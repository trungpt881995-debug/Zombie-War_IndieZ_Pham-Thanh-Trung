using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Ports
{
    public sealed class NullCameraShakeDriver : ICameraShakeDriver
    {
        public static NullCameraShakeDriver Instance { get; } = new NullCameraShakeDriver();
        private NullCameraShakeDriver() { }
        public bool TryPlay(in CameraShakeRequest request) => false;
        public void StopAll() { }
    }
}
