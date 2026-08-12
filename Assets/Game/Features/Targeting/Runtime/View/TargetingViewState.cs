using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.View
{
    public readonly struct TargetingViewState
    {
        public EntityId OwnerId { get; }
        public bool HasTarget { get; }
        public EntityId TargetId { get; }
        public TargetPoint TargetPosition { get; }
        public TargetLossReason LastLossReason { get; }

        public TargetingViewState(EntityId ownerId, in TargetingResult result, TargetLossReason lastLossReason)
        {
            OwnerId = ownerId;
            HasTarget = result.HasTarget;
            TargetId = result.TargetId;
            TargetPosition = result.TargetPosition;
            LastLossReason = lastLossReason;
        }
    }
}
