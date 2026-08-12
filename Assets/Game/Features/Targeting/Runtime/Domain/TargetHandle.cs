using System;
using GameplayCore.Entities;

namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Captures the entity identity at acquisition time. This protects a retained
    /// target from silently becoming a different pooled entity if a backing object
    /// is reused with another EntityId.
    /// </summary>
    public readonly struct TargetHandle
    {
        public EntityId EntityId { get; }
        public ITargetCandidate Candidate { get; }

        public TargetHandle(ITargetCandidate candidate)
        {
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            EntityId = candidate.EntityId;
        }
    }
}
