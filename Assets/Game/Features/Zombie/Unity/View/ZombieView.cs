using System;
using UnityEngine;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.Unity.View
{
    [DisallowMultipleComponent]
    public sealed class ZombieView : MonoBehaviour, IZombieView
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider[] gameplayColliders;
        [SerializeField] private Renderer[] dissolveRenderers;
        [SerializeField] private string dissolveProperty = "_DissolveAmount";

        [Header("Animation Priority")]
        [Tooltip(
            "Animator state name used for forced death priority. " +
            "Keep this equal to the actual Death state name on layer 0.")]
        [SerializeField] private string deathStateName = "Death";

        [Tooltip(
            "Small fixed cross-fade used when lethal damage arrives while Hit/Attack " +
            "is still active. Set to 0 for an immediate transition.")]
        [SerializeField, Min(0f)] private float deathCrossFadeDuration = 0.05f;

        private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
        private static readonly int SpawnHash = Animator.StringToHash("Spawn");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private MaterialPropertyBlock _propertyBlock;
        private int _dissolveId;
        private int _deathStateHash;

        public event Action AttackImpact;
        public event Action AttackFinished;
        public event Action HitFinished;
        public event Action DeathFinished;

        public ZombiePoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new ZombiePoint(p.x, p.y, p.z);
            }
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
                _deathStateHash = ResolveStateHash(animator, deathStateName);
            }

            _propertyBlock = new MaterialPropertyBlock();
            _dissolveId = Shader.PropertyToID(dissolveProperty);
        }

        public void ResetForReuse()
        {
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.speed = 1f;
                animator.Rebind();
                animator.Update(0f);
                animator.SetFloat(MovementSpeedHash, 0f);
                ResetTransientTriggers();
                _deathStateHash = ResolveStateHash(animator, deathStateName);
            }

            SetDissolveProgress(0f);
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        public void SetLocomotionSpeed(float normalizedSpeed)
        {
            if (animator != null)
            {
                animator.SetFloat(
                    MovementSpeedHash,
                    Mathf.Clamp01(normalizedSpeed));
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
            in ZombiePoint target,
            float rotationSpeed,
            float deltaTime)
        {
            Vector3 direction = new Vector3(
                target.X - transform.position.x,
                0f,
                target.Z - transform.position.z);

            if (direction.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Quaternion desired = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                Mathf.Max(0f, rotationSpeed) * Mathf.Max(0f, deltaTime));
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
            if (animator == null)
            {
                return;
            }

            // Death is terminal gameplay state. Lethal flame ticks frequently arrive
            // while the Animator is still in Hit/Attack transition. Clear every
            // competing one-shot trigger before requesting Death.
            animator.speed = 1f;
            animator.SetFloat(MovementSpeedHash, 0f);
            ResetTransientTriggers();

            // Prefer direct state transition so Death wins even when the current
            // Animator state has no transition to Death. This specifically prevents
            // continuous Flamethrower hit reactions from visually swallowing Death.
            if (_deathStateHash != 0)
            {
                animator.CrossFadeInFixedTime(
                    _deathStateHash,
                    Mathf.Max(0f, deathCrossFadeDuration),
                    0,
                    0f);
                return;
            }

            // Backward-compatible fallback for controllers whose Death state has a
            // different name but is correctly reachable through the existing trigger.
            animator.SetTrigger(DeathHash);
        }

        public void SetDissolveProgress(float normalizedProgress)
        {
            if (dissolveRenderers == null || _propertyBlock == null)
            {
                return;
            }

            float value = Mathf.Clamp01(normalizedProgress);
            for (int i = 0; i < dissolveRenderers.Length; i++)
            {
                Renderer renderer = dissolveRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_dissolveId, value);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetAnimationPaused(bool paused)
        {
            if (animator != null)
            {
                animator.speed = paused ? 0f : 1f;
            }
        }

        // Add these methods as Unity Animation Events at the appropriate clips.
        public void AnimationEvent_AttackImpact() => AttackImpact?.Invoke();
        public void AnimationEvent_AttackFinished() => AttackFinished?.Invoke();
        public void AnimationEvent_HitFinished() => HitFinished?.Invoke();
        public void AnimationEvent_DeathFinished() => DeathFinished?.Invoke();

        private void ResetTransientTriggers()
        {
            if (animator == null)
            {
                return;
            }

            animator.ResetTrigger(SpawnHash);
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(HitHash);
            animator.ResetTrigger(DeathHash);
        }

        private static int ResolveStateHash(
            Animator targetAnimator,
            string stateName)
        {
            if (targetAnimator == null || string.IsNullOrWhiteSpace(stateName))
            {
                return 0;
            }

            const int layerIndex = 0;
            string layerName = targetAnimator.GetLayerName(layerIndex);
            string fullPath = string.IsNullOrEmpty(layerName)
                ? stateName
                : layerName + "." + stateName;

            int fullPathHash = Animator.StringToHash(fullPath);
            if (targetAnimator.HasState(layerIndex, fullPathHash))
            {
                return fullPathHash;
            }

            int shortHash = Animator.StringToHash(stateName);
            return targetAnimator.HasState(layerIndex, shortHash)
                ? shortHash
                : 0;
        }
    }
}
