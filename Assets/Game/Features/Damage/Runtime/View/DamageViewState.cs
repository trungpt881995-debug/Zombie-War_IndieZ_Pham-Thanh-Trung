using GameplayCore.Entities;
using ZombieWar.Features.Damage.Domain;

namespace ZombieWar.Features.Damage.View
{
    public readonly struct DamageViewState
    {
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public float RequestedAmount { get; }
        public float FinalAmount { get; }
        public string Type { get; }
        public bool Accepted { get; }
        public DamageRejectionReason RejectionReason { get; }

        public DamageViewState(in DamageResolution result)
        {
            SourceId = result.SourceId;
            TargetId = result.TargetId;
            RequestedAmount = result.RequestedAmount;
            FinalAmount = result.FinalAmount;
            Type = result.Type;
            Accepted = result.Accepted;
            RejectionReason = result.RejectionReason;
        }
    }
}
