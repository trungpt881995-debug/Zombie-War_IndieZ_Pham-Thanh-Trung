using UnityEngine;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Unity.Runtime
{
    /// <summary>
    /// Development/smoke-test fallback. Production should use OptionalCinemachine/CinemachineCameraRig.
    /// This rig intentionally performs no smoothing so smoothing is owned by Cinemachine in production.
    /// </summary>
    public sealed class TransformCameraRig : CameraRigBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private UnityEngine.Camera unityCamera;

        private CameraProfile _profile;
        private bool _runtimeEnabled;

        public override bool IsReady => cameraTransform != null;

        private void Awake()
        {
            if (cameraTransform == null && unityCamera != null) cameraTransform = unityCamera.transform;
            if (unityCamera == null && cameraTransform != null) unityCamera = cameraTransform.GetComponent<UnityEngine.Camera>();
        }

        public override void ApplyProfile(in CameraProfile profile)
        {
            _profile = profile;
            if (cameraTransform != null)
                cameraTransform.rotation = Quaternion.Euler(profile.Pitch, profile.Yaw, profile.Roll);
            if (unityCamera != null)
            {
                unityCamera.orthographic = profile.ProjectionMode == CameraProjectionMode.Orthographic;
                unityCamera.fieldOfView = profile.FieldOfView;
                unityCamera.orthographicSize = profile.OrthographicSize;
                unityCamera.nearClipPlane = profile.NearClip;
                unityCamera.farClipPlane = profile.FarClip;
            }
        }

        public override void SetTarget(in CameraPoint target)
        {
            if (!_runtimeEnabled || cameraTransform == null) return;
            Place(in target);
        }

        public override void SnapToTarget(in CameraPoint target)
        {
            if (cameraTransform == null) return;
            Place(in target);
        }

        public override void SetEnabled(bool enabled) => _runtimeEnabled = enabled;

        private void Place(in CameraPoint target)
        {
            cameraTransform.position = new Vector3(
                target.X + _profile.OffsetX,
                target.Y + _profile.OffsetY,
                target.Z + _profile.OffsetZ);
            cameraTransform.rotation = Quaternion.Euler(_profile.Pitch, _profile.Yaw, _profile.Roll);
        }
    }
}
