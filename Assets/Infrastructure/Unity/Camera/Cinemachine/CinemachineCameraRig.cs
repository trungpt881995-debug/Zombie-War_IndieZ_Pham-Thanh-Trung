using UnityEngine;
using Unity.Cinemachine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Unity.Runtime;

namespace ZombieWar.Infrastructure.Camera.Cinemachine
{
    public sealed class CinemachineCameraRig : CameraRigBehaviour
    {
        private const float MinCameraDistance = 0.01f;

        [Header("Cinemachine")]
        [SerializeField]
        private CinemachineCamera virtualCamera;

        [SerializeField]
        private CinemachinePositionComposer positionComposer;

        [SerializeField]
        private Transform constrainedTarget;

        private CameraProfile _profile;

        public override bool IsReady =>
            virtualCamera != null &&
            positionComposer != null &&
            constrainedTarget != null;

        private void Awake()
        {
            BindTrackingTarget();
        }

        public override void ApplyProfile(
            in CameraProfile profile)
        {
            _profile = profile;

            if (!IsReady)
            {
                return;
            }

            BindTrackingTarget();

            // CameraProfile still exposes the legacy Follow Offset vector.
            // Position Composer uses a scalar CameraDistance instead, so preserve
            // the existing authored shot scale by converting the vector magnitude.
            float cameraDistance = GetLegacyOffsetMagnitude(profile);
            if (cameraDistance >= MinCameraDistance)
            {
                positionComposer.CameraDistance = cameraDistance;
            }

            // Damping semantics remain compatible with Position Composer.
            positionComposer.Damping = new Vector3(
                profile.DampingX,
                profile.DampingY,
                profile.DampingZ);

            // Intentionally do not overwrite Position Composer Composition,
            // TargetOffset, DeadZoneDepth, Lookahead, or CenterOnActivate here.
            // CameraProfile does not currently own those Position Composer settings,
            // so they remain authored on the CinemachinePositionComposer component.

            LensSettings lens = virtualCamera.Lens;
            lens.ModeOverride =
                profile.ProjectionMode == CameraProjectionMode.Orthographic
                    ? LensSettings.OverrideModes.Orthographic
                    : LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = profile.FieldOfView;
            lens.OrthographicSize = profile.OrthographicSize;
            lens.NearClipPlane = profile.NearClip;
            lens.FarClipPlane = profile.FarClip;
            lens.Dutch = profile.Roll;
            lens.Validate();
            virtualCamera.Lens = lens;

            // Rotation Control remains None. The fixed angled top-down orientation
            // is owned explicitly by CameraProfile.
            virtualCamera.transform.rotation = Quaternion.Euler(
                profile.Pitch,
                profile.Yaw,
                profile.Roll);
        }

        public override void SetTarget(
            in CameraPoint target)
        {
            if (!IsReady || !virtualCamera.enabled)
            {
                return;
            }

            constrainedTarget.position = new Vector3(
                target.X,
                target.Y,
                target.Z);
        }

        public override void SnapToTarget(
            in CameraPoint target)
        {
            if (!IsReady)
            {
                return;
            }

            Vector3 targetPosition = new Vector3(
                target.X,
                target.Y,
                target.Z);

            constrainedTarget.position = targetPosition;

            Quaternion rotation = Quaternion.Euler(
                _profile.Pitch,
                _profile.Yaw,
                _profile.Roll);

            float cameraDistance = Mathf.Max(
                MinCameraDistance,
                positionComposer.CameraDistance);

            // Position Composer tracks TargetOffset in target-local space.
            Vector3 trackedPoint =
                targetPosition +
                constrainedTarget.TransformVector(positionComposer.TargetOffset);

            // Position Composer keeps the tracked point CameraDistance units in
            // front of the camera along the camera's local forward axis.
            Vector3 cameraPosition =
                trackedPoint -
                (rotation * Vector3.forward * cameraDistance);

            // ForceCameraPosition propagates the snap through the Cinemachine
            // component pipeline, including Position Composer's internal state.
            virtualCamera.ForceCameraPosition(
                cameraPosition,
                rotation);
        }

        public override void SetEnabled(
            bool enabled)
        {
            if (virtualCamera != null)
            {
                virtualCamera.enabled = enabled;
            }
        }

        private void BindTrackingTarget()
        {
            if (virtualCamera == null || constrainedTarget == null)
            {
                return;
            }

            // Cinemachine 3 Position Composer consumes the camera Tracking Target.
            // Use the explicit target field instead of depending on a
            // CinemachineFollow component.
            virtualCamera.Target.TrackingTarget = constrainedTarget;
        }

        private static float GetLegacyOffsetMagnitude(
            in CameraProfile profile)
        {
            Vector3 legacyOffset = new Vector3(
                profile.OffsetX,
                profile.OffsetY,
                profile.OffsetZ);

            return legacyOffset.magnitude;
        }
    }
}
