using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Targeting.Events
{
    public readonly struct TargetAcquiredEvent : IEvent
    {
        public EntityId OwnerId { get; }
        public EntityId TargetId { get; }

        public TargetAcquiredEvent(EntityId ownerId, EntityId targetId)
        {
            OwnerId = ownerId;
            TargetId = targetId;
        }
    }
}
