using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombiePoolReturnPort
    {
        void Return(EntityId entityId, ZombieReleaseReason reason);
    }
}
