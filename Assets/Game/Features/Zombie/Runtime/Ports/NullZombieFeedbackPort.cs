using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public sealed class NullZombieFeedbackPort : IZombieFeedbackPort
    {
        public static readonly NullZombieFeedbackPort Instance = new NullZombieFeedbackPort();
        private NullZombieFeedbackPort() { }
        public void OnHit(EntityId zombieId, in ZombiePoint position) { }
        public void OnDeath(EntityId zombieId, in ZombiePoint position) { }
    }
}
