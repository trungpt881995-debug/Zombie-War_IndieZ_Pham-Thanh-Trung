using UnityEngine;

namespace ZombieWar.Features.Soldier.View
{
    /// <summary>
    /// Presentation-only procedural torso aim.
    /// Animator evaluates first, then LateUpdate applies additive yaw/pitch to
    /// Humanoid torso bones. Root/Hips/Legs are never rotated here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoldierUpperBodyAim : MonoBehaviour
    {
        [Header("Aim Limits")]
        [SerializeField, Range(0f, 180f)]
        private float maxYaw = 110f;

        [SerializeField, Range(0f, 89f)]
        private float maxPitchUp = 35f;

        [SerializeField, Range(0f, 89f)]
        private float maxPitchDown = 30f;

        [Header("Torso Distribution")]
        [SerializeField, Range(0f, 1f)]
        private float spineWeight = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float chestWeight = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float upperChestWeight = 0.40f;

        [Header("Optional Head")]
        [SerializeField]
        private bool includeHead;

        [SerializeField, Range(0f, 0.5f)]
        private float headWeight = 0.10f;

        private Animator _animator;
        private Transform _characterRoot;
        private Transform _spine;
        private Transform _chest;
        private Transform _upperChest;
        private Transform _head;

        private bool _hasAim;
        private Vector3 _smoothedWorldDirection;

        public bool IsBound =>
            _animator != null &&
            _characterRoot != null;

        public void Bind(
            Animator animator,
            Transform characterRoot)
        {
            _animator = animator;
            _characterRoot = characterRoot != null
                ? characterRoot
                : transform;

            ResolveHumanoidBones();
        }

        public void SetAimDirection(
            Vector3 worldDirection,
            float rotationDegreesPerSecond,
            float deltaTime)
        {
            if (worldDirection.sqrMagnitude <= 0.000001f)
            {
                ClearAim();
                return;
            }

            Vector3 desired = worldDirection.normalized;

            if (!_hasAim ||
                _smoothedWorldDirection.sqrMagnitude <= 0.000001f ||
                rotationDegreesPerSecond <= 0f ||
                deltaTime <= 0f)
            {
                _smoothedWorldDirection = desired;
            }
            else
            {
                float maxRadians =
                    rotationDegreesPerSecond *
                    Mathf.Deg2Rad *
                    deltaTime;

                _smoothedWorldDirection = Vector3.RotateTowards(
                    _smoothedWorldDirection,
                    desired,
                    maxRadians,
                    0f).normalized;
            }

            _hasAim = true;
        }

        public void ClearAim()
        {
            _hasAim = false;
            _smoothedWorldDirection = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (!_hasAim ||
                !IsBound ||
                _smoothedWorldDirection.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            if (_spine == null &&
                _chest == null &&
                _upperChest == null)
            {
                ResolveHumanoidBones();
            }

            Vector3 localDirection =
                _characterRoot.InverseTransformDirection(
                    _smoothedWorldDirection.normalized);

            float horizontal = Mathf.Sqrt(
                localDirection.x * localDirection.x +
                localDirection.z * localDirection.z);

            float yaw = Mathf.Atan2(
                localDirection.x,
                localDirection.z) * Mathf.Rad2Deg;

            float pitch = Mathf.Atan2(
                localDirection.y,
                Mathf.Max(0.0001f, horizontal)) * Mathf.Rad2Deg;

            yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
            pitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);

            ApplyBoneOffset(_spine, spineWeight, yaw, pitch);
            ApplyBoneOffset(_chest, chestWeight, yaw, pitch);
            ApplyBoneOffset(_upperChest, upperChestWeight, yaw, pitch);

            if (includeHead)
                ApplyBoneOffset(_head, headWeight, yaw, pitch);
        }

        private void ApplyBoneOffset(
            Transform bone,
            float weight,
            float yaw,
            float pitch)
        {
            if (bone == null || weight <= 0f)
                return;

            Quaternion yawOffset = Quaternion.AngleAxis(
                yaw * weight,
                _characterRoot.up);

            Vector3 pitchAxis =
                yawOffset * _characterRoot.right;

            // Negative sign: positive target elevation should tilt the torso upward.
            Quaternion pitchOffset = Quaternion.AngleAxis(
                -pitch * weight,
                pitchAxis);

            bone.rotation =
                pitchOffset *
                yawOffset *
                bone.rotation;
        }

        private void ResolveHumanoidBones()
        {
            _spine = null;
            _chest = null;
            _upperChest = null;
            _head = null;

            if (_animator == null || !_animator.isHuman)
                return;

            _spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
            _upperChest = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
            _head = _animator.GetBoneTransform(HumanBodyBones.Head);
        }

        private void OnDisable()
        {
            ClearAim();
        }
    }
}
