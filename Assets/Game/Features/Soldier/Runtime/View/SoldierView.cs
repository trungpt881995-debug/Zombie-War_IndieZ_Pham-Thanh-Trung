using UnityEngine;
using ZombieWar.Features.Soldier.Animation;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    public sealed class SoldierView :
        MonoBehaviour,
        ISoldierView,
        ISoldierFacingView,
        ISoldierWeaponAnimationView
    {
        [Header("Animation")]
        [SerializeField]
        private Animator animator;

        [Tooltip("Optional legacy aim pivot. SoldierUpperBodyAim takes priority when available.")]
        [SerializeField]
        private Transform aimPivot;

        [Tooltip("Procedural torso aim applied after Animator evaluation. Auto-created at runtime when missing.")]
        [SerializeField]
        private SoldierUpperBodyAim upperBodyAim;

        [SerializeField]
        private bool autoCreateUpperBodyAim = true;

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
                Vector3 p = CachedTransform.position;
                return new SoldierPoint(p.x, p.y, p.z);
            }
        }

        public SoldierDirection Forward
        {
            get
            {
                Vector3 forward = CachedTransform.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude <= 0.000001f)
                    return SoldierDirection.Zero;

                forward.Normalize();
                return new SoldierDirection(forward.x, 0f, forward.z);
            }
        }

        private Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                    _cachedTransform = transform;

                return _cachedTransform;
            }
        }

        private void Awake()
        {
            _cachedTransform = transform;

            // Keep compatibility with scenes where the Animator reference was not
            // serialized yet, without changing the public SoldierView contract.
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            _movementSpeedHash = Animator.StringToHash(movementSpeedParameter);
            _aimXHash = Animator.StringToHash(aimXParameter);
            _aimYHash = Animator.StringToHash(aimYParameter);
            _hasTargetHash = Animator.StringToHash(hasTargetParameter);
            _shootTriggerHash = Animator.StringToHash(shootTriggerParameter);

            // Mandatory Zombie War rule.
            if (animator != null)
                animator.applyRootMotion = false;

            if (upperBodyAim == null)
                upperBodyAim = GetComponent<SoldierUpperBodyAim>();

            if (upperBodyAim == null && autoCreateUpperBodyAim)
                upperBodyAim = gameObject.AddComponent<SoldierUpperBodyAim>();

            if (upperBodyAim != null)
                upperBodyAim.Bind(animator, CachedTransform);
        }

        public void SetActive(bool active)
        {
            if (gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }

        public void SetLocalFormationPosition(
            in SoldierPoint localPosition)
        {
            CachedTransform.localPosition = new Vector3(
                localPosition.X,
                localPosition.Y,
                localPosition.Z);
        }

        public void SetBodyFacing(
            in SoldierDirection direction,
            float rotationDegreesPerSecond,
            float deltaTime)
        {
            if (!direction.HasDirection)
                return;

            Vector3 planarDirection = new Vector3(
                direction.X,
                0f,
                direction.Z);

            if (planarDirection.sqrMagnitude <= 0.000001f)
                return;

            planarDirection.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(
                planarDirection,
                Vector3.up);

            if (rotationDegreesPerSecond <= 0f || deltaTime <= 0f)
            {
                CachedTransform.rotation = targetRotation;
                return;
            }

            CachedTransform.rotation = Quaternion.RotateTowards(
                CachedTransform.rotation,
                targetRotation,
                rotationDegreesPerSecond * deltaTime);
        }

        /// <summary>
        /// Backward-compatible alias retained for scene/runtime code authored before
        /// body-facing became target-aware.
        /// </summary>
        public void SetMovementFacing(
            in SoldierDirection direction,
            float rotationDegreesPerSecond,
            float deltaTime)
        {
            SetBodyFacing(
                in direction,
                rotationDegreesPerSecond,
                deltaTime);
        }

        /// <summary>
        /// Signed locomotion input:
        /// +1 = Run Forward, +0.5 = Walk Forward,
        ///  0 = Idle,
        /// -0.5 = Walk Backward, -1 = Run Backward.
        /// </summary>
        public void SetMovementSpeed(float normalizedSpeed)
        {
            if (animator == null)
                return;

            animator.SetFloat(
                _movementSpeedHash,
                Mathf.Clamp(normalizedSpeed, -1f, 1f));
        }

        public void SetAimDirection(
            in SoldierDirection direction,
            float rotationDegreesPerSecond,
            float deltaTime)
        {
            if (!direction.HasDirection)
            {
                ClearAim();
                return;
            }

            Vector3 worldDirection = new Vector3(
                direction.X,
                direction.Y,
                direction.Z).normalized;

            if (animator != null)
            {
                Vector3 localDirection =
                    CachedTransform.InverseTransformDirection(worldDirection);

                // Preserve the existing Animator contract: AimX/AimY remain a
                // planar XZ directional blend. Vertical pitch is procedural.
                Vector2 planar = new Vector2(
                    localDirection.x,
                    localDirection.z);

                if (planar.sqrMagnitude > 0.000001f)
                    planar.Normalize();

                animator.SetBool(_hasTargetHash, true);
                animator.SetFloat(_aimXHash, planar.x);
                animator.SetFloat(_aimYHash, planar.y);
            }

            if (upperBodyAim != null)
            {
                upperBodyAim.SetAimDirection(
                    worldDirection,
                    rotationDegreesPerSecond,
                    deltaTime);
                return;
            }

            // Backward-compatible fallback if procedural upper-body aim is disabled.
            if (aimPivot == null)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(
                worldDirection,
                Vector3.up);

            if (rotationDegreesPerSecond <= 0f || deltaTime <= 0f)
            {
                aimPivot.rotation = targetRotation;
                return;
            }

            aimPivot.rotation = Quaternion.RotateTowards(
                aimPivot.rotation,
                targetRotation,
                rotationDegreesPerSecond * deltaTime);
        }

        public void ClearAim()
        {
            if (animator != null)
            {
                animator.SetBool(_hasTargetHash, false);
                animator.SetFloat(_aimXHash, 0f);
                animator.SetFloat(_aimYHash, 0f);
            }

            if (upperBodyAim != null)
                upperBodyAim.ClearAim();
        }

        public void PlayShoot()
        {
            if (animator == null)
                return;

            // Reset first so a rapid weapon can restart the upper-body Shoot state
            // on every accepted shot instead of waiting for the previous clip to finish.
            animator.ResetTrigger(_shootTriggerHash);
            animator.SetTrigger(_shootTriggerHash);
        }
    }
}
