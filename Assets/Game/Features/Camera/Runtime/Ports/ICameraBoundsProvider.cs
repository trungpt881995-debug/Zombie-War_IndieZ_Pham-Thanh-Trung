using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Ports
{
    public interface ICameraBoundsProvider
    {
        bool TryGetBounds(out CameraBounds bounds);
    }
}
