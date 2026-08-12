using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.Ports
{
    public interface IMovementIntentSink
    {
        void Set(in MovementIntent intent);
    }
}
