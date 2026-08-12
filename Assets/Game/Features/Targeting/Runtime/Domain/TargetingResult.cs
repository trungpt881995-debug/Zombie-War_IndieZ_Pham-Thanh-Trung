using GameplayCore.Entities;

namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Snapshot returned to the owner. It deliberately exposes no Zombie/Boss
    /// controller, GameObject or Transform reference.
    /// </summary>
    public readonly struct TargetingResult
    {
        public static readonly TargetingResult None = new TargetingResult(false, default, default);

        public bool HasTarget { get; }
        public EntityId TargetId { get; }
        public TargetPoint TargetPosition { get; }

        private TargetingResult(bool hasTarget, EntityId targetId, TargetPoint targetPosition)
        {
            HasTarget = hasTarget;
            TargetId = targetId;
            TargetPosition = targetPosition;
        }

        public static TargetingResult From(in TargetHandle handle)
        {
            return new TargetingResult(true, handle.EntityId, handle.Candidate.Position);
        }
    }
}
