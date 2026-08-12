using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Events
{
    public readonly struct ZombieReleasedEvent : IEvent
    {
        public EntityId ZombieId { get; }
        public ZombieReleaseReason Reason { get; }
        public ZombieReleasedEvent(EntityId zombieId, ZombieReleaseReason reason)
        {
            ZombieId = zombieId; Reason = reason;
        }
    }
}
