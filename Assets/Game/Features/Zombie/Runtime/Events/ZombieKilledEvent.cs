using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Events
{
    public readonly struct ZombieKilledEvent : IEvent
    {
        public EntityId ZombieId { get; }
        public EntityId KillerId { get; }
        public ZombieKilledEvent(EntityId zombieId, EntityId killerId)
        {
            ZombieId = zombieId; KillerId = killerId;
        }
    }
}
