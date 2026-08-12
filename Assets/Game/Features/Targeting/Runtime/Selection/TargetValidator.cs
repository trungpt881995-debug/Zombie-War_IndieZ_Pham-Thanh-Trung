using System;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Registry;

namespace ZombieWar.Features.Targeting.Selection
{
    /// <summary>
    /// Validates retention of an already-acquired target.
    /// </summary>
    public sealed class TargetValidator : ITargetValidator
    {
        private readonly ITargetRegistry _registry;
        private readonly IDistanceMetric _distance;

        public TargetValidator(ITargetRegistry registry, IDistanceMetric distance)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));

            _distance = distance ?? throw new ArgumentNullException(nameof(distance));
        }

        public TargetLossReason Validate(in TargetHandle target, in TargetingContext context)
        {
            ITargetCandidate candidate = target.Candidate;

            if (candidate == null)
                return TargetLossReason.MissingCandidate;

            if (candidate.EntityId != target.EntityId)
                return TargetLossReason.EntityIdentityChanged;

            if (!_registry.Contains(target.EntityId))
                return TargetLossReason.Unregistered;

            if (!candidate.IsTargetable)
                return TargetLossReason.NotTargetable;

            float sqrDistance = _distance.SqrDistance(context.Origin, candidate.Position);

            if (sqrDistance > context.RangeSquared)
                return TargetLossReason.OutOfRange;

            return TargetLossReason.None;
        }
    }
}
