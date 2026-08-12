using ZombieWar.Features.Camera.Catalog;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Services
{
    public interface ICameraRuntimeConfigurator
    {
        void Initialize(
            in CameraProfile profile,
            ICameraShakeCatalog shakeCatalog,
            ICameraTargetProvider targetProvider,
            ICameraBoundsProvider boundsProvider,
            ICameraRig rig,
            ICameraShakeDriver shakeDriver);

        void Shutdown();
    }
}
