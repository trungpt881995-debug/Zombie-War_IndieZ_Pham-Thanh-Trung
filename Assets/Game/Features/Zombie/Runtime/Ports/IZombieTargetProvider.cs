using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieTargetProvider
    {
        bool TryAcquireTarget(in ZombiePoint zombiePosition, out ZombieTarget target);
        bool TryGetTarget(EntityId entityId, out ZombieTarget target);
    }
}
