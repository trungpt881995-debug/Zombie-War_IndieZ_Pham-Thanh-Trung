using System;
using UnityEngine;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;

namespace ZombieWar.Features.Boss.Unity.View
{
    [DisallowMultipleComponent] public sealed class BossView : MonoBehaviour, IBossView
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider[] gameplayColliders;
        [Header("Attack Impact Fallback")][Tooltip("Fallback seconds from PlayAttack() to the melee impact. " + "A real AnimationEvent_AttackImpact still wins when present.")][SerializeField, Min(0.01f)] private float attackImpactFallbackDelay = 0.4f;
        private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
        private static readonly int SpawnHash = Animator.StringToHash("Spawn");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private const int BaseAnimatorLayer = 0;
        private const float DeathCompleteNormalizedTime = 0.999f;
        private bool _deathCompletionPending;
        private bool _deathStateCaptured;
        private bool _deathFinishedRaised;
        private bool _deathStateEntered;
        private int _stateBeforeDeathHash;
        private int _preDeathTransitionDestinationHash;
        private int _deathStateHash;
        private bool _attackImpactPending;
        private bool _attackImpactRaised;
        private float _attackImpactRemaining;
        public event Action AttackImpact;
        public event Action AttackFinished;
        public event Action HitFinished;
        public event Action DeathFinished;
        public BossPoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new BossPoint(p.x, p.y, p.z);
            }
        }
        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren < Animator > ();
            }
            if (animator == null)
            {
                return;
            }
            animator.applyRootMotion = false;
            EnsureAnimationEventReceiver();
        }
        private void Update()
        {
            PollAttackImpactFallback();
            PollDeathAnimationCompletion();
        }
        public void ResetForReuse()
        {
            ResetAttackImpactTracking();
            ResetDeathCompletionTracking();
            if (animator == null)
            {
                return;
            }
            EnsureAnimationEventReceiver();
            animator.applyRootMotion = false;
            animator.speed = 1f;
            animator.Rebind();
            animator.Update(0f);
            animator.SetFloat(MovementSpeedHash, 0f);
        }
        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        }
        public void SetLocomotionSpeed(float speed)
        {
            if (animator != null)
            {
                animator.SetFloat(MovementSpeedHash, Mathf.Clamp01(speed));
            }
        }
        public void SetGameplayCollisionEnabled(bool enabled)
        {
            if (gameplayColliders == null)
            {
                return;
            }
            for (int i = 0; i < gameplayColliders.Length; i++)
            {
                if (gameplayColliders[i] != null)
                {
                    gameplayColliders[i].enabled = enabled;
                }
            }
        }
        public void FaceTarget(in BossPoint target, float rotationSpeed, float dt)
        {
            Vector3 d = new Vector3(target.X - transform.position.x, 0f, target.Z - transform.position.z);
            if (d.sqrMagnitude <= 0.000001f)
            {
                return;
            }
            Quaternion q = Quaternion.LookRotation(d.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, Mathf.Max(0f, rotationSpeed) * Mathf.Max(0f,
            dt));
        }
        public void PlaySpawn()
        {
            ResetAttackImpactTracking();
            if (animator != null)
            {
                animator.SetTrigger(SpawnHash);
            }
        }
        public void PlayAttack()
        {
            BeginAttackImpactTracking();
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }
        }
        public void PlayHit()
        {
            ResetAttackImpactTracking();
            if (animator != null)
            {
                animator.SetTrigger(HitHash);
            }
        }
        public void PlayDeath()
        {
            ResetAttackImpactTracking();
            ResetDeathCompletionTracking();
            _deathCompletionPending = true;
            if (animator == null)
            {
                // There is no visual death animation to wait for.
                RaiseDeathFinishedOnce();
                return;
            }
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);
            _stateBeforeDeathHash = current.fullPathHash;
            _preDeathTransitionDestinationHash = 0;
            // A lethal hit can arrive while the Animator is already blending into
            // Hit/Attack. Remember that pre-existing destination so the death-state
            // detector cannot accidentally capture it as the Death state.
            if (animator.IsInTransition(BaseAnimatorLayer))
            {
                AnimatorStateInfo preDeathNext = animator.GetNextAnimatorStateInfo(BaseAnimatorLayer);
                _preDeathTransitionDestinationHash = preDeathNext.fullPathHash;
            }
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(HitHash);
            animator.SetTrigger(DeathHash);
        }
        public void SetAnimationPaused(bool paused)
        {
            if (animator != null)
            {
                animator.speed = paused ? 0f : 1f;
            }
        }
        public void AnimationEvent_AttackImpact()
        {
            RaiseAttackImpactOnce();
        }
        public void AnimationEvent_AttackFinished()
        {
            ResetAttackImpactTracking();
            AttackFinished?.Invoke();
        }
        public void AnimationEvent_HitFinished()
        {
            HitFinished?.Invoke();
        }
        public void AnimationEvent_DeathFinished()
        {
            RaiseDeathFinishedOnce();
        }
        private void BeginAttackImpactTracking()
        {
            _attackImpactPending = true;
            _attackImpactRaised = false;
            _attackImpactRemaining = Mathf.Max(0.01f, attackImpactFallbackDelay);
        }
        private void PollAttackImpactFallback()
        {
            if (!_attackImpactPending || _attackImpactRaised)
            {
                return;
            }
            _attackImpactRemaining -= Time.deltaTime;
            if (_attackImpactRemaining <= 0f)
            {
                RaiseAttackImpactOnce();
            }
        }
        private void RaiseAttackImpactOnce()
        {
            if (!_attackImpactPending || _attackImpactRaised)
            {
                return;
            }
            _attackImpactRaised = true;
            _attackImpactPending = false;
            AttackImpact?.Invoke();
        }
        private void ResetAttackImpactTracking()
        {
            _attackImpactPending = false;
            _attackImpactRaised = false;
            _attackImpactRemaining = 0f;
        }
        private void PollDeathAnimationCompletion()
        {
            if (!_deathCompletionPending || _deathFinishedRaised || animator == null || !animator.isActiveAndEnabled || animator.layerCount <= BaseAnimatorLayer)
            {
                return;
            }
            if (!_deathStateCaptured)
            {
                TryCaptureDeathState();
            }
            if (!_deathStateCaptured)
            {
                return;
            }
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);
            if (current.fullPathHash == _deathStateHash)
            {
                _deathStateEntered = true;
                if (current.normalizedTime >= DeathCompleteNormalizedTime)
                {
                    RaiseDeathFinishedOnce();
                }
                return;
            }
            // Once the captured Death state has actually become the current state,
            // leaving it is also a valid visual-completion signal. This covers
            // controllers whose Death state transitions to Exit/Idle slightly
            // before Update observes normalizedTime >= 0.999.
            if (_deathStateEntered)
            {
                RaiseDeathFinishedOnce();
            }
        }
        private void TryCaptureDeathState()
        {
            if (animator.IsInTransition(BaseAnimatorLayer))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(BaseAnimatorLayer);
                if (IsDeathStateCandidate(next.fullPathHash))
                {
                    _deathStateHash = next.fullPathHash;
                    _deathStateCaptured = true;
                }
                return;
            }
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);
            if (IsDeathStateCandidate(current.fullPathHash))
            {
                _deathStateHash = current.fullPathHash;
                _deathStateCaptured = true;
            }
        }
        private bool IsDeathStateCandidate(int stateHash)
        {
            return stateHash != 0 && stateHash != _stateBeforeDeathHash && stateHash != _preDeathTransitionDestinationHash;
        }
        private void RaiseDeathFinishedOnce()
        {
            if (!_deathCompletionPending || _deathFinishedRaised)
            {
                return;
            }
            _deathFinishedRaised = true;
            _deathCompletionPending = false;
            DeathFinished?.Invoke();
        }
        private void ResetDeathCompletionTracking()
        {
            _deathCompletionPending = false;
            _deathStateCaptured = false;
            _deathFinishedRaised = false;
            _deathStateEntered = false;
            _stateBeforeDeathHash = 0;
            _preDeathTransitionDestinationHash = 0;
            _deathStateHash = 0;
        }
        private void EnsureAnimationEventReceiver()
        {
            if (animator == null || animator.gameObject == gameObject)
            {
                return;
            }
            BossAnimationEventRelay relay = animator.GetComponent < BossAnimationEventRelay > ();
            if (relay == null)
            {
                relay = animator.gameObject.AddComponent < BossAnimationEventRelay > ();
            }
            relay.Bind(this);
        }
    }
}
