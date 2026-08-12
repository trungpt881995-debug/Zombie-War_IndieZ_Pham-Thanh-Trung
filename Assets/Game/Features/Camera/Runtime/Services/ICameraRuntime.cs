using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Services
{
    public interface ICameraRuntime
    {
        bool IsInitialized { get; }
        CameraState State { get; }
        bool GameplayEnabled { get; }
        bool HasTarget { get; }
        bool HasBounds { get; }
        CameraPoint RawTarget { get; }
        CameraPoint ConstrainedTarget { get; }
        CameraProfile Profile { get; }

        void Tick(float deltaTime);
        void SetGameplayEnabled(bool enabled);
        bool SnapToTarget();
        void ApplyProfile(in CameraProfile profile);
        bool TryRequestShake(CameraShakeId shakeId);
    }
}
