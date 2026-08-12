using UnityEngine;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Unity.Runtime;

namespace ZombieWar.Infrastructure.Camera.Cinemachine
{
    public sealed class CinemachineCameraRig : CameraRigBehaviour
    {
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private CinemachineFollow follow;
        [SerializeField] private Transform constrainedTarget;

        private CameraProfile _profile;

        public override bool IsReady =>
            virtualCamera != null && follow != null && constrainedTarget != null;

        private void Awake()
        {
            if (virtualCamera != null && constrainedTarget != null)
                virtualCamera.Follow = constrainedTarget;
        }

        public override void ApplyProfile(in CameraProfile profile)
        {
            _profile = profile;
            if (!IsReady) return;

            virtualCamera.Follow = constrainedTarget;
            follow.FollowOffset = new Vector3(profile.OffsetX, profile.OffsetY, profile.OffsetZ);

            TrackerSettings tracker = follow.TrackerSettings;
            tracker.BindingMode = BindingMode.WorldSpace;
            tracker.PositionDamping = new Vector3(profile.DampingX, profile.DampingY, profile.DampingZ);
            tracker.Validate();
            follow.TrackerSettings = tracker;

            LensSettings lens = virtualCamera.Lens;
            lens.ModeOverride = profile.ProjectionMode == CameraProjectionMode.Orthographic
                ? LensSettings.OverrideModes.Orthographic
                : LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = profile.FieldOfView;
            lens.OrthographicSize = profile.OrthographicSize;
            lens.NearClipPlane = profile.NearClip;
            lens.FarClipPlane = profile.FarClip;
            lens.Dutch = profile.Roll;
            lens.Validate();
            virtualCamera.Lens = lens;

            virtualCamera.transform.rotation = Quaternion.Euler(profile.Pitch, profile.Yaw, profile.Roll);
        }

        public override void SetTarget(in CameraPoint target)
        {
            if (!IsReady || !virtualCamera.enabled) return;
            constrainedTarget.position = new Vector3(target.X, target.Y, target.Z);
        }

        public override void SnapToTarget(in CameraPoint target)
        {
            if (!IsReady) return;
            Vector3 targetPosition = new Vector3(target.X, target.Y, target.Z);
            constrainedTarget.position = targetPosition;
            Quaternion rotation = Quaternion.Euler(_profile.Pitch, _profile.Yaw, _profile.Roll);
            Vector3 cameraPosition = targetPosition + follow.FollowOffset;
            follow.ForceCameraPosition(cameraPosition, rotation);
            virtualCamera.transform.SetPositionAndRotation(cameraPosition, rotation);
        }

        public override void SetEnabled(bool enabled)
        {
            if (virtualCamera != null) virtualCamera.enabled = enabled;
        }
    }
}
