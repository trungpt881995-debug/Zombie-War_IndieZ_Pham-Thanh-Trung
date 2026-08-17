using System;
using UnityEngine;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;

namespace ZombieWar.Features.Boss.Unity.View
{
    [DisallowMultipleComponent]
    public sealed class BossView : MonoBehaviour, IBossView
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider[] gameplayColliders;

        private static readonly int MovementSpeedHash =
            Animator.StringToHash("MovementSpeed");
        private static readonly int SpawnHash =
            Animator.StringToHash("Spawn");
        private static readonly int AttackHash =
            Animator.StringToHash("Attack");
        private static readonly int HitHash =
            Animator.StringToHash("Hit");
        private static readonly int DeathHash =
            Animator.StringToHash("Death");

        private const int BaseAnimatorLayer = 0;
        private const float DeathCompleteNormalizedTime = 0.999f;

        private bool _deathCompletionPending;
        private bool _deathStateCaptured;
        private bool _deathFinishedRaised;
        private int _stateBeforeDeathHash;
        private int _deathStateHash;

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
                animator = GetComponentInChildren<Animator>();
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
            PollDeathAnimationCompletion();
        }

        public void ResetForReuse()
        {
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
            transform.localScale =
                Vector3.one * Mathf.Max(0.01f, scale);
        }

        public void SetLocomotionSpeed(float speed)
        {
            if (animator != null)
            {
                animator.SetFloat(
                    MovementSpeedHash,
                    Mathf.Clamp01(speed));
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

        public void FaceTarget(
            in BossPoint target,
            float rotationSpeed,
            float dt)
        {
            Vector3 d = new Vector3(
                target.X - transform.position.x,
                0f,
                target.Z - transform.position.z);

            if (d.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Quaternion q = Quaternion.LookRotation(
                d.normalized,
                Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                q,
                Mathf.Max(0f, rotationSpeed) * Mathf.Max(0f, dt));
        }

        public void PlaySpawn()
        {
            if (animator != null)
            {
                animator.SetTrigger(SpawnHash);
            }
        }

        public void PlayAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }
        }

        public void PlayHit()
        {
            if (animator != null)
            {
                animator.SetTrigger(HitHash);
            }
        }

        public void PlayDeath()
        {
            ResetDeathCompletionTracking();
            _deathCompletionPending = true;

            if (animator == null)
            {
                // There is no visual death animation to wait for.
                RaiseDeathFinishedOnce();
                return;
            }

            AnimatorStateInfo current =
                animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);

            _stateBeforeDeathHash = current.fullPathHash;
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
            AttackImpact?.Invoke();
        }

        public void AnimationEvent_AttackFinished()
        {
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

        private void PollDeathAnimationCompletion()
        {
            if (!_deathCompletionPending ||
                _deathFinishedRaised ||
                animator == null ||
                !animator.isActiveAndEnabled ||
                animator.layerCount <= BaseAnimatorLayer)
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

            // While blending into Death, the destination state may already be
            // the captured Death state. Do not complete until it becomes the
            // current state and reaches the end of its first playback.
            if (animator.IsInTransition(BaseAnimatorLayer))
            {
                return;
            }

            AnimatorStateInfo current =
                animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);

            if (current.fullPathHash != _deathStateHash)
            {
                return;
            }

            if (current.normalizedTime >= DeathCompleteNormalizedTime)
            {
                RaiseDeathFinishedOnce();
            }
        }

        private void TryCaptureDeathState()
        {
            if (animator.IsInTransition(BaseAnimatorLayer))
            {
                AnimatorStateInfo next =
                    animator.GetNextAnimatorStateInfo(BaseAnimatorLayer);

                if (next.fullPathHash != 0 &&
                    next.fullPathHash != _stateBeforeDeathHash)
                {
                    _deathStateHash = next.fullPathHash;
                    _deathStateCaptured = true;
                }

                return;
            }

            AnimatorStateInfo current =
                animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer);

            if (current.fullPathHash != 0 &&
                current.fullPathHash != _stateBeforeDeathHash)
            {
                _deathStateHash = current.fullPathHash;
                _deathStateCaptured = true;
            }
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
            _stateBeforeDeathHash = 0;
            _deathStateHash = 0;
        }

        private void EnsureAnimationEventReceiver()
        {
            if (animator == null || animator.gameObject == gameObject)
            {
                return;
            }

            BossAnimationEventRelay relay =
                animator.GetComponent<BossAnimationEventRelay>();

            if (relay == null)
            {
                relay = animator.gameObject
                    .AddComponent<BossAnimationEventRelay>();
            }

            relay.Bind(this);
        }
    }
}
