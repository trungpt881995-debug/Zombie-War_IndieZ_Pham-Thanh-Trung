using UnityEngine;
using UnityEngine.AI;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.Unity.Movement
{
    /// <summary>
    /// Unity NavMesh implementation of IZombieMotor.
    ///
    /// Design goals:
    /// - Keep Zombie domain/state-machine code independent from Unity Navigation.
    /// - Use NavMesh pathfinding for static obstacle avoidance.
    /// - Use NavMeshAgent local avoidance for zombie-vs-zombie avoidance.
    /// - Avoid SetDestination every frame by throttling repath requests.
    /// - Be safe with the existing Zombie object-pool lifecycle.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class NavMeshZombieMotor : MonoBehaviour, IZombieMotor, IZombieSteeringProvider
    {
        [Header("Agent Shape")]
        [SerializeField, Min(0.01f)] private float radius = 0.32f;
        [SerializeField, Min(0.01f)] private float height = 1.8f;
        [SerializeField] private float baseOffset = 0f;

        [Header("Agent Motion")]
        [SerializeField, Min(0f)] private float acceleration = 12f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.05f;
        [SerializeField] private bool autoBraking = false;
        [SerializeField] private bool autoRepath = true;

        [Header("Local Avoidance")]
        [SerializeField] private ObstacleAvoidanceType obstacleAvoidanceType =
            ObstacleAvoidanceType.LowQualityObstacleAvoidance;

        [Tooltip("Base avoidance priority. Lower values have higher priority in Unity NavMesh avoidance.")]
        [SerializeField, Range(0, 99)] private int avoidancePriority = 50;

        [Tooltip("Adds a deterministic per-zombie spread around the base priority to reduce crowd deadlocks.")]
        [SerializeField, Range(0, 40)] private int avoidancePrioritySpread = 20;

        [Header("Path Recalculation")]
        [Tooltip("Minimum gameplay-time interval between path requests while chasing a moving target.")]
        [SerializeField, Min(0.02f)] private float repathInterval = 0.12f;

        [Tooltip("Target must move at least this far from the last requested destination before a normal repath.")]
        [SerializeField, Min(0f)] private float destinationChangeThreshold = 0.15f;

        [Tooltip("How far around the requested target point we may search for a valid NavMesh point.")]
        [SerializeField, Min(0.01f)] private float destinationSampleDistance = 1.5f;

        [Header("Spawn / Recovery")]
        [Tooltip("How far around a spawn request we may search for the nearest valid NavMesh point.")]
        [SerializeField, Min(0.01f)] private float warpSampleDistance = 2f;

        [Tooltip("Small recovery radius used if an enabled agent temporarily loses its NavMesh binding.")]
        [SerializeField, Min(0.01f)] private float recoverySampleDistance = 1f;

        [SerializeField] private bool logNavigationWarnings = true;

        private NavMeshAgent _agent;
        private bool _movementEnabled = true;
        private bool _hasRequestedDestination;
        private Vector3 _lastRequestedDestination;
        private float _repathRemaining;
        private float _lastCommandedSpeed;
        private bool _warnedMissingNavMesh;

        public ZombiePoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new ZombiePoint(p.x, p.y, p.z);
            }
        }

        public float NormalizedSpeed
        {
            get
            {
                if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || _agent.isStopped)
                    return 0f;

                float denominator = Mathf.Max(0.0001f, _lastCommandedSpeed);
                return Mathf.Clamp01(_agent.velocity.magnitude / denominator);
            }
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            ApplyAgentSettings();
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
                NavMeshAgent agent = GetComponent<NavMeshAgent>();
                if (agent != null)
                    ApplyAgentSettings(agent);
            }
        }

        public void Warp(in ZombiePoint position)
        {
            EnsureAgent();
            ResetRuntimeNavigationState();

            Vector3 requested = new Vector3(position.X, position.Y, position.Z);

            // Sampling is agent-type aware. This also protects pooled zombies from being
            // spawned slightly outside the baked NavMesh because of spawn-sector geometry.
            if (!TrySample(requested, warpSampleDistance, out Vector3 sampled))
            {
                // Keep the requested transform position for diagnostics/fallback visuals,
                // but do not issue NavMesh commands until a valid binding can be recovered.
                SetAgentComponentEnabled(false);
                transform.position = requested;
                WarnMissingNavMeshOnce(
                    $"Zombie spawn position {requested} has no NavMesh within {warpSampleDistance:0.##}m.");
                return;
            }

            // Rebinding through disable -> move -> enable is robust for pooled agents that
            // were disabled on Death/Cancel and are activated again at another location.
            SetAgentComponentEnabled(false);
            transform.position = sampled;
            SetAgentComponentEnabled(true);

            if (_agent.isOnNavMesh)
            {
                // Warp keeps the NavMesh simulation position synchronized immediately.
                _agent.Warp(sampled);
                _agent.isStopped = true;
                _warnedMissingNavMesh = false;
            }
            else
            {
                WarnMissingNavMeshOnce(
                    $"NavMeshAgent could not bind at sampled spawn position {sampled}.");
            }
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

            if (!_agent.enabled)
                SetAgentComponentEnabled(true);

            if (!_agent.isOnNavMesh && !TryRecoverBinding())
            {
                WarnMissingNavMeshOnce("Zombie NavMeshAgent is enabled but is not on a baked NavMesh.");
                return;
            }

            _agent.isStopped = true;
            _warnedMissingNavMesh = false;
        }

        public void MoveTowards(in ZombiePoint target, float speed, float deltaTime)
        {
            if (!_movementEnabled || deltaTime <= 0f)
            {
                Stop();
                return;
            }

            EnsureAgent();

            if (!_agent.enabled)
                SetAgentComponentEnabled(true);

            if (!_agent.isOnNavMesh && !TryRecoverBinding())
            {
                WarnMissingNavMeshOnce("Zombie cannot chase because its NavMeshAgent is not bound to a NavMesh.");
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
            bool targetMovedEnough = !_hasRequestedDestination ||
                                     (rawTarget - _lastRequestedDestination).sqrMagnitude >= threshold * threshold;

            bool pathNeedsRepair = !_agent.pathPending &&
                                   (!_agent.hasPath || _agent.isPathStale ||
                                    _agent.pathStatus == NavMeshPathStatus.PathInvalid);

            bool intervalElapsed = _repathRemaining <= 0f;
            if (!_hasRequestedDestination || pathNeedsRepair || (intervalElapsed && targetMovedEnough))
            {
                RequestDestination(rawTarget);
            }
        }

        public bool TryGetSteeringTarget(out ZombiePoint target)
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh &&
                !_agent.pathPending && _agent.hasPath)
            {
                Vector3 steering = _agent.steeringTarget;
                target = new ZombiePoint(steering.x, steering.y, steering.z);
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
                return;

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void RequestDestination(Vector3 rawTarget)
        {
            _repathRemaining = repathInterval;

            if (!TrySample(rawTarget, destinationSampleDistance, out Vector3 sampledTarget))
                return;

            if (_agent.SetDestination(sampledTarget))
            {
                // Store raw target so thresholding tracks the Soldier's actual movement,
                // rather than tiny differences produced by repeated NavMesh sampling.
                _lastRequestedDestination = rawTarget;
                _hasRequestedDestination = true;
            }
        }

        private bool TryRecoverBinding()
        {
            if (_agent == null)
                return false;

            Vector3 current = transform.position;
            if (!TrySample(current, recoverySampleDistance, out Vector3 sampled))
                return false;

            SetAgentComponentEnabled(false);
            transform.position = sampled;
            SetAgentComponentEnabled(true);

            if (!_agent.isOnNavMesh)
                return false;

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
                agentTypeID = _agent.agentTypeID,
                areaMask = _agent.areaMask
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
                return;

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
                _agent = GetComponent<NavMeshAgent>();
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
                return;

            agent.radius = radius;
            agent.height = height;
            agent.baseOffset = baseOffset;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = autoBraking;
            agent.autoRepath = autoRepath;

            // Rotation remains owned by ZombieView.FaceTarget(), preserving the existing
            // presentation contract and preventing NavMeshAgent/View rotation conflicts.
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            agent.obstacleAvoidanceType = obstacleAvoidanceType;
            agent.avoidancePriority = ResolveAvoidancePriority();
        }

        private int ResolveAvoidancePriority()
        {
            if (avoidancePrioritySpread <= 0)
                return avoidancePriority;

            int half = avoidancePrioritySpread / 2;
            int hash = GetInstanceID() & 0x7fffffff;
            int offset = hash % (avoidancePrioritySpread + 1) - half;
            return Mathf.Clamp(avoidancePriority + offset, 0, 99);
        }

        private void SetAgentComponentEnabled(bool enabled)
        {
            if (_agent == null || _agent.enabled == enabled)
                return;

            _agent.enabled = enabled;
        }

        private void WarnMissingNavMeshOnce(string message)
        {
            if (!logNavigationWarnings || _warnedMissingNavMesh)
                return;

            _warnedMissingNavMesh = true;
            Debug.LogWarning($"[ZombieWar][ZombieNavigation] {message}", this);
        }
    }
}
