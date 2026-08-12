using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Ports
{
    public interface ICameraShakeDriver
    {
        bool TryPlay(in CameraShakeRequest request);
        void StopAll();
    }
}
