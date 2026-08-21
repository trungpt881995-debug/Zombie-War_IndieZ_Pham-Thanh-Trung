using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;

namespace ZombieWar.Features.Boss.Unity.Movement
{
    /// <summary>
    /// NavMesh implementation of IBossMotor.
    ///
    /// Responsibilities:
    /// - Pathfind around baked static obstacles.
    /// - Participate in NavMeshAgent local avoidance.
    /// - Give Bosses higher avoidance priority than the Zombie crowd by default.
    /// - Throttle SetDestination requests while following moving Soldiers.
    /// - Safely sample/warp pooled Bosses onto the NavMesh.
    /// - Recover an agent that temporarily loses its NavMesh binding.
    ///
    /// Boss domain/state-machine code remains independent from Unity Navigation.
    /// </summary>
    [DisallowMultipleComponent][RequireComponent(typeof(NavMeshAgent))] public sealed class NavMeshBossMotor : MonoBehaviour, IBossMotor, IBossSteeringProvider
    {
        [Header("Agent Shape - tune per Boss prefab")][Tooltip("Local-avoidance radius. For narrow passages, remember baked Agent Type radius also controls static-world clearance.")][SerializeField, Min(0.01f)] private float radius = 0.75f;
        [SerializeField, Min(0.01f)] private float height = 2.5f;
        [SerializeField] private float baseOffset = 0f;
        [Header("Agent Motion")][SerializeField, Min(0f)] private float acceleration = 10f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;
        [SerializeField] private bool autoBraking = false;
        [SerializeField] private bool autoRepath = true;
        [Header("Local Avoidance")][SerializeField] private ObstacleAvoidanceType obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        [Tooltip("Lower number = higher avoidance priority. Boss defaults above Zombie crowd priority (Zombie baseline is 50).")][SerializeField, Range(0,
        99)] private int avoidancePriority = 20;
        [Tooltip("Small deterministic spread prevents two Bosses from having identical avoidance priority.")][SerializeField, Range(0,
        20)] private int avoidancePrioritySpread = 8;
        [Header("Path Recalculation")][Tooltip("Minimum gameplay-time interval between path requests while chasing a moving Soldier.")][SerializeField, Min(0.02f)] private float repathInterval = 0.1f;
        [Tooltip("Target must move at least this far before a normal repath is requested.")][SerializeField, Min(0f)] private float destinationChangeThreshold = 0.2f;
        [Tooltip("Nearest NavMesh search radius around the Soldier target point.")][SerializeField, Min(0.01f)] private float destinationSampleDistance = 2f;
        [Header("Spawn / Recovery")][Tooltip("Nearest NavMesh search radius around the requested Boss spawn point.")][SerializeField, Min(0.01f)] private float warpSampleDistance = 3f;
        [Tooltip("Small search radius used if an enabled Boss loses its NavMesh binding.")][SerializeField, Min(0.01f)] private float recoverySampleDistance = 1.5f;
        [SerializeField] private bool logNavigationWarnings = true;
        private NavMeshAgent _agent;
        private bool _movementEnabled;
        private bool _hasRequestedDestination;
        private Vector3 _lastRequestedDestination;
        private float _repathRemaining;
        private float _lastCommandedSpeed;
        private bool _warnedMissingNavMesh;
        public BossPoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new BossPoint(p.x, p.y, p.z);
            }
        }
        public float NormalizedSpeed
        {
            get
            {
                if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || _agent.isStopped)
                {
                    return 0f;
                }
                float denominator = Mathf.Max(0.0001f, _lastCommandedSpeed);
                return Mathf.Clamp01(_agent.velocity.magnitude / denominator);
            }
        }
        private void Reset()
        {
            _agent = GetComponent < NavMeshAgent > ();
            ApplyAgentSettings();
            // Important for prefab pooling: a pooled Agent should not try to bind before
            // BossController.Activate() has sampled and warped it onto a valid NavMesh.
            if (_agent != null)
            {
                _agent.enabled = false;
            }
        }
        private void Awake()
        {
            _agent = GetComponent < NavMeshAgent > ();
            // Make the first pool prewarm safe even if the prefab was accidentally saved
            // with NavMeshAgent enabled. The prefab should still be saved disabled because
            // Unity may try to create native agent state before MonoBehaviour.Awake runs.
            if (_agent != null && _agent.enabled)
            {
                _agent.enabled = false;
            }
            _movementEnabled = false;
            ApplyAgentSettings();
        }
        private void OnDisable()
        {
            _movementEnabled = false;
            ResetRuntimeNavigationState();
            // Persist the safe pooled state across GameObject disable/enable cycles.
            if (_agent != null && _agent.enabled)
            {
                _agent.enabled = false;
            }
        }
        private void OnValidate()
        {
            radius = Mathf.Max(0.01f, radius);
            height = Mathf.Max(0.01f, height);
            acceleration = Mathf.Max(0f, acceleration);
            stoppingDistance = Mathf.Max(0f, stoppingDistance);
            repathInterval = Mathf.Max(0.02f, repathInterval);
            destinationChangeThreshold = Mathf.Max(0f, destinationChangeThreshold);
            destinationSampleDistance = Mathf.Max(0.01f, destinationSampleDistance);
            warpSampleDistance = Mathf.Max(0.01f, warpSampleDistance);
            recoverySampleDistance = Mathf.Max(0.01f, recoverySampleDistance);
            if (!Application.isPlaying)
            {
                NavMeshAgent agent = GetComponent < NavMeshAgent > ();
                if (agent != null)
                {
                    ApplyAgentSettings(agent);
                }
            }
        }
        public void Warp(in BossPoint position)
        {
            EnsureAgent();
            ResetRuntimeNavigationState();
            // BossView.SetScale() is called before Motor.Warp() in BossController.Activate().
            // Re-apply serialized agent values here so prefab-specific navigation tuning is
            // guaranteed to be active after the Boss visual scale has been assigned.
            ApplyAgentSettings();
            Vector3 requested = new Vector3(position.X, position.Y, position.Z);
            if (!TrySample(requested, warpSampleDistance, out Vector3 sampled))
            {
                SetAgentComponentEnabled(false);
                transform.position = requested;
                WarnMissingNavMeshOnce($"Boss spawn position {requested} has no NavMesh within {warpSampleDistance:0.##}m.");
                return;
            }
            SetAgentComponentEnabled(false);
            transform.position = sampled;
            SetAgentComponentEnabled(true);
            if (!_agent.isOnNavMesh)
            {
                SetAgentComponentEnabled(false);
                WarnMissingNavMeshOnce($"NavMeshAgent could not bind Boss at sampled spawn position {sampled}.");
                return;
            }
            _agent.Warp(sampled);
            _agent.isStopped = true;
            _warnedMissingNavMesh = false;
        }
        public void SetEnabled(bool enabled)
        {
            EnsureAgent();
            _movementEnabled = enabled;
            if (!enabled)
            {
                Stop();
                SetAgentComponentEnabled(false);
                return;
            }
            if (!EnsureBoundAgent())
            {
                WarnMissingNavMeshOnce("Boss NavMeshAgent cannot be enabled because no valid baked NavMesh is available near the Boss.");
                return;
            }
            _agent.isStopped = true;
            _warnedMissingNavMesh = false;
        }
        public void MoveTowards(in BossPoint target, float speed, float deltaTime)
        {
            if (!_movementEnabled || deltaTime <= 0f)
            {
                Stop();
                return;
            }
            EnsureAgent();
            if (!EnsureBoundAgent())
            {
                WarnMissingNavMeshOnce("Boss cannot chase because its NavMeshAgent is not bound to a valid NavMesh.");
                return;
            }
            _lastCommandedSpeed = Mathf.Max(0f, speed);
            _agent.speed = _lastCommandedSpeed;
            if (_lastCommandedSpeed <= 0f)
            {
                StopAgentWithoutDisabling();
                return;
            }
            _agent.isStopped = false;
            _repathRemaining -= deltaTime;
            Vector3 rawTarget = new Vector3(target.X, target.Y, target.Z);
            float threshold = destinationChangeThreshold;
            bool targetMovedEnough = !_hasRequestedDestination || (rawTarget - _lastRequestedDestination).sqrMagnitude >= threshold * threshold;
            bool pathNeedsRepair = !_agent.pathPending && (!_agent.hasPath || _agent.isPathStale || _agent.pathStatus == NavMeshPathStatus.PathInvalid);
            bool intervalElapsed = _repathRemaining <= 0f;
            if (!_hasRequestedDestination || pathNeedsRepair || (intervalElapsed && targetMovedEnough))
            {
                RequestDestination(rawTarget);
            }
        }
        public bool TryGetSteeringTarget(out BossPoint target)
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh && !_agent.pathPending && _agent.hasPath)
            {
                Vector3 steering = _agent.steeringTarget;
                target = new BossPoint(steering.x, steering.y, steering.z);
                return true;
            }
            target = default;
            return false;
        }
        public void Stop()
        {
            _lastCommandedSpeed = 0f;
            _hasRequestedDestination = false;
            _repathRemaining = 0f;
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }
            _agent.isStopped = true;
            _agent.ResetPath();
        }
        private void RequestDestination(Vector3 rawTarget)
        {
            _repathRemaining = repathInterval;
            if (!TrySample(rawTarget, destinationSampleDistance, out Vector3 sampledTarget))
            {
                return;
            }
            if (_agent.SetDestination(sampledTarget))
            {
                // Threshold against the real Soldier target, not the sampled point, so
                // tiny projection differences do not trigger needless path calculations.
                _lastRequestedDestination = rawTarget;
                _hasRequestedDestination = true;
            }
        }
        private bool EnsureBoundAgent()
        {
            if (_agent == null)
            {
                return false;
            }
            if (_agent.enabled && _agent.isOnNavMesh)
            {
                return true;
            }
            // Never blindly enable an off-mesh agent. Sample first, then enable only at a
            // location known to contain compatible NavMesh data. This avoids warning spam
            // when a map has not loaded/baked navigation yet.
            if (_agent.enabled)
            {
                SetAgentComponentEnabled(false);
            }
            if (!TrySample(transform.position, recoverySampleDistance, out Vector3 sampled))
            {
                return false;
            }
            transform.position = sampled;
            SetAgentComponentEnabled(true);
            if (!_agent.isOnNavMesh)
            {
                SetAgentComponentEnabled(false);
                return false;
            }
            _agent.Warp(sampled);
            _agent.isStopped = true;
            _hasRequestedDestination = false;
            _repathRemaining = 0f;
            _warnedMissingNavMesh = false;
            return true;
        }
        private bool TrySample(Vector3 source, float maxDistance, out Vector3 sampled)
        {
            EnsureAgent();
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = _agent.agentTypeID, areaMask = _agent.areaMask
            };
            if (NavMesh.SamplePosition(source, out NavMeshHit hit, maxDistance, filter))
            {
                sampled = hit.position;
                return true;
            }
            sampled = default;
            return false;
        }
        private void StopAgentWithoutDisabling()
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }
            _agent.isStopped = true;
        }
        private void ResetRuntimeNavigationState()
        {
            _hasRequestedDestination = false;
            _lastRequestedDestination = default;
            _repathRemaining = 0f;
            _lastCommandedSpeed = 0f;
        }
        private void EnsureAgent()
        {
            if (_agent == null)
            {
                _agent = GetComponent < NavMeshAgent > ();
                ApplyAgentSettings();
            }
        }
        private void ApplyAgentSettings()
        {
            ApplyAgentSettings(_agent);
        }
        private void ApplyAgentSettings(NavMeshAgent agent)
        {
            if (agent == null)
            {
                return;
            }
            agent.radius = radius;
            agent.height = height;
            agent.baseOffset = baseOffset;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = autoBraking;
            agent.autoRepath = autoRepath;
            // BossView.FaceTarget() remains the sole visual rotation owner.
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.obstacleAvoidanceType = obstacleAvoidanceType;
            agent.avoidancePriority = ResolveAvoidancePriority();
        }
        private int ResolveAvoidancePriority()
        {
            if (avoidancePrioritySpread <= 0)
            {
                return avoidancePriority;
            }
            int half = avoidancePrioritySpread / 2;
            int hash = GetInstanceID()& 0x7fffffff;
            int offset = hash % (avoidancePrioritySpread + 1) - half;
            return Mathf.Clamp(avoidancePriority + offset, 0, 99);
        }
        private void SetAgentComponentEnabled(bool enabled)
        {
            if (_agent == null || _agent.enabled == enabled)
            {
                return;
            }
            _agent.enabled = enabled;
        }
        private void WarnMissingNavMeshOnce(string message)
        {
            if (!logNavigationWarnings || _warnedMissingNavMesh)
            {
                return;
            }
            _warnedMissingNavMesh = true;
            Debug.LogWarning($"[ZombieWar][BossNavigation] {message}", this);
        }
    }
}
