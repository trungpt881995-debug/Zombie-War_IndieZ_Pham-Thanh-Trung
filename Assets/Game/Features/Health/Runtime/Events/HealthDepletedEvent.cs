using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Health.Events
{
    public readonly struct HealthDepletedEvent : IEvent
    {
        public EntityId OwnerId { get; }

        public HealthDepletedEvent(EntityId ownerId)
        {
            OwnerId = ownerId;
        }
    }
}
