using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Ports
{
    public interface ICameraTargetProvider
    {
        bool TryGetTarget(out CameraPoint position);
    }
}
