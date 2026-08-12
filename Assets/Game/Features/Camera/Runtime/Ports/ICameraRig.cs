using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Ports
{
    public interface ICameraRig
    {
        bool IsReady { get; }
        void ApplyProfile(in CameraProfile profile);
        void SetTarget(in CameraPoint target);
        void SnapToTarget(in CameraPoint target);
        void SetEnabled(bool enabled);
    }
}
