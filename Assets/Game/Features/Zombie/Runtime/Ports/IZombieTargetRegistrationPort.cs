using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieTargetRegistrationPort
    {
        void Register(EntityId entityId);
        void Unregister(EntityId entityId);
    }
}
