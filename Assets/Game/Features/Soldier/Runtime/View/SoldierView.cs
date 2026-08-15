using UnityEngine;
using ZombieWar.Features.Soldier.Animation;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    public sealed class SoldierView :
        MonoBehaviour,
        ISoldierView,
        ISoldierWeaponAnimationView
    {
        [Header("Animation")]
        [SerializeField]
        private Animator animator;

        [Tooltip("Optional non-animated aim pivot. Leave null when aiming is entirely driven by the upper-body Animator layer.")]
        [SerializeField]
        private Transform aimPivot;

        [SerializeField]
        private string movementSpeedParameter =
            SoldierAnimatorContract.MovementSpeed;

        [SerializeField]
        private string aimXParameter =
            SoldierAnimatorContract.AimX;

        [SerializeField]
        private string aimYParameter =
            SoldierAnimatorContract.AimY;

        [SerializeField]
        private string hasTargetParameter =
            SoldierAnimatorContract.HasTarget;

        [SerializeField]
        private string shootTriggerParameter =
            SoldierAnimatorContract.Shoot;

        private Transform _cachedTransform;

        private int _movementSpeedHash;
        private int _aimXHash;
        private int _aimYHash;
        private int _hasTargetHash;
        private int _shootTriggerHash;

        public SoldierPoint Position
        {
            get
            {
                Transform t = CachedTransform;
                Vector3 p = t.position;

                return new SoldierPoint(
                    p.x,
                    p.y,
                    p.z);
            }
        }

        private Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = transform;
                }

                return _cachedTransform;
            }
        }

        private void Awake()
        {
            _cachedTransform = transform;

            _movementSpeedHash =
                Animator.StringToHash(
                    movementSpeedParameter);

            _aimXHash =
                Animator.StringToHash(
                    aimXParameter);

            _aimYHash =
                Animator.StringToHash(
                    aimYParameter);

            _hasTargetHash =
                Animator.StringToHash(
                    hasTargetParameter);

            _shootTriggerHash =
                Animator.StringToHash(
                    shootTriggerParameter);

            // Mandatory Zombie War rule.
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        public void SetLocalFormationPosition(
            in SoldierPoint localPosition)
        {
            CachedTransform.localPosition =
                new Vector3(
                    localPosition.X,
                    localPosition.Y,
                    localPosition.Z);
        }

        public void SetMovementSpeed(
            float normalizedSpeed)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(
                _movementSpeedHash,
                Mathf.Clamp01(normalizedSpeed));
        }

        public void SetAimDirection(
            in SoldierDirection direction,
            float rotationDegreesPerSecond,
            float deltaTime)
        {
            if (animator != null)
            {
                Vector3 worldDirection =
                    new Vector3(
                        direction.X,
                        direction.Y,
                        direction.Z);

                Vector3 localDirection =
                    CachedTransform.InverseTransformDirection(
                        worldDirection);

                animator.SetBool(
                    _hasTargetHash,
                    true);

                animator.SetFloat(
                    _aimXHash,
                    localDirection.x);

                animator.SetFloat(
                    _aimYHash,
                    localDirection.z);
            }

            if (aimPivot == null ||
                !direction.HasDirection)
            {
                return;
            }

            Vector3 flatDirection =
                new Vector3(
                    direction.X,
                    0f,
                    direction.Z);

            if (flatDirection.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    flatDirection,
                    Vector3.up);

            if (rotationDegreesPerSecond <= 0f ||
                deltaTime <= 0f)
            {
                aimPivot.rotation = targetRotation;
                return;
            }

            aimPivot.rotation =
                Quaternion.RotateTowards(
                    aimPivot.rotation,
                    targetRotation,
                    rotationDegreesPerSecond *
                    deltaTime);
        }

        public void ClearAim()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(
                _hasTargetHash,
                false);

            animator.SetFloat(
                _aimXHash,
                0f);

            animator.SetFloat(
                _aimYHash,
                0f);
        }

        public void PlayShoot()
        {
            if (animator == null)
            {
                return;
            }

            // Reset first so a rapid weapon can restart the upper-body Shoot state
            // on every accepted shot instead of waiting for the previous clip to finish.
            animator.ResetTrigger(_shootTriggerHash);
            animator.SetTrigger(_shootTriggerHash);
        }
    }
}
