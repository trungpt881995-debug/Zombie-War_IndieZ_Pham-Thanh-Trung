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

        private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
        private static readonly int SpawnHash = Animator.StringToHash("Spawn");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private MaterialPropertyBlock _propertyBlock;
        private int _dissolveId;

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
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.applyRootMotion = false;
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
            }
            SetDissolveProgress(0f);
        }
        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active) gameObject.SetActive(active);
        }
        public void SetLocomotionSpeed(float normalizedSpeed)
        {
            if (animator != null) animator.SetFloat(MovementSpeedHash, Mathf.Clamp01(normalizedSpeed));
        }
        public void SetGameplayCollisionEnabled(bool enabled)
        {
            if (gameplayColliders == null) return;
            for (int i = 0; i < gameplayColliders.Length; i++)
                if (gameplayColliders[i] != null) gameplayColliders[i].enabled = enabled;
        }
        public void FaceTarget(in ZombiePoint target, float rotationSpeed, float deltaTime)
        {
            Vector3 direction = new Vector3(target.X - transform.position.x, 0f, target.Z - transform.position.z);
            if (direction.sqrMagnitude <= 0.000001f) return;
            Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, Mathf.Max(0f, rotationSpeed) * Mathf.Max(0f, deltaTime));
        }
        public void PlaySpawn() { if (animator != null) animator.SetTrigger(SpawnHash); }
        public void PlayAttack() { if (animator != null) animator.SetTrigger(AttackHash); }
        public void PlayHit() { if (animator != null) animator.SetTrigger(HitHash); }
        public void PlayDeath() { if (animator != null) animator.SetTrigger(DeathHash); }
        public void SetDissolveProgress(float normalizedProgress)
        {
            if (dissolveRenderers == null || _propertyBlock == null) return;
            float value = Mathf.Clamp01(normalizedProgress);
            for (int i = 0; i < dissolveRenderers.Length; i++)
            {
                Renderer renderer = dissolveRenderers[i];
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_dissolveId, value);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
        public void SetAnimationPaused(bool paused) { if (animator != null) animator.speed = paused ? 0f : 1f; }

        // Add these methods as Unity Animation Events at the appropriate clips.
        public void AnimationEvent_AttackImpact() => AttackImpact?.Invoke();
        public void AnimationEvent_AttackFinished() => AttackFinished?.Invoke();
        public void AnimationEvent_HitFinished() => HitFinished?.Invoke();
        public void AnimationEvent_DeathFinished() => DeathFinished?.Invoke();
    }
}
