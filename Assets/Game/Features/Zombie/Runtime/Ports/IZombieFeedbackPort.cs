using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieFeedbackPort
    {
        void OnHit(EntityId zombieId, in ZombiePoint position);
        void OnDeath(EntityId zombieId, in ZombiePoint position);
    }
}
