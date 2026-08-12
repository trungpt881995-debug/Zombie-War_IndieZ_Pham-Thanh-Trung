using GameplayCore.Entities;

namespace ZombieWar.Features.Damage.Domain
{
    /// <summary>
    /// Immutable result of validating/resolving a damage request.
    /// RequestedAmount and FinalAmount are intentionally separated so future
    /// rules can be introduced without changing callers or IDamageService.
    /// </summary>
    public readonly struct DamageResolution
    {
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public float RequestedAmount { get; }
        public float FinalAmount { get; }
        public string Type { get; }
        public bool Accepted { get; }
        public DamageRejectionReason RejectionReason { get; }

        public DamageResolution(EntityId sourceId, EntityId targetId, float requestedAmount, float finalAmount, string type, bool accepted, DamageRejectionReason rejectionReason)
        {
            SourceId = sourceId;
            TargetId = targetId;
            RequestedAmount = requestedAmount;
            FinalAmount = finalAmount;
            Type = type ?? "Default";
            Accepted = accepted;
            RejectionReason = rejectionReason;
        }
    }
}
