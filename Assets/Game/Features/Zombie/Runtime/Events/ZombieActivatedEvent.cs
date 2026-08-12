using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Events
{
    public readonly struct ZombieActivatedEvent : IEvent
    {
        public EntityId ZombieId { get; }
        public ZombieActivatedEvent(EntityId zombieId) => ZombieId = zombieId;
    }
}
