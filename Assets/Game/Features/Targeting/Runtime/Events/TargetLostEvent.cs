using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Events
{
    public readonly struct TargetLostEvent : IEvent
    {
        public EntityId OwnerId { get; }
        public EntityId TargetId { get; }
        public TargetLossReason Reason { get; }

        public TargetLostEvent(EntityId ownerId, EntityId targetId, TargetLossReason reason)
        {
            OwnerId = ownerId;
            TargetId = targetId;
            Reason = reason;
        }
    }
}
