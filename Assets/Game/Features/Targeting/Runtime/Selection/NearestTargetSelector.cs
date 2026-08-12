using System;
using System.Collections.Generic;
using GameplayCore.Targeting;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Selection
{
    /// <summary>
    /// Strategy Pattern implementation for Zombie War:
    /// nearest target inside TargetRange from THIS Soldier's origin.
    /// </summary>
    public sealed class NearestTargetSelector : ITargetSelector<TargetingContext, ITargetCandidate>
    {
        private readonly IDistanceMetric _distance;

        public NearestTargetSelector(IDistanceMetric distance)
        {
            _distance = distance ?? throw new ArgumentNullException(nameof(distance));
        }

        public ITargetCandidate Select(TargetingContext context, IReadOnlyList<ITargetCandidate> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            ITargetCandidate best = null;
            float bestSqrDistance = context.RangeSquared;

            for (int i = 0; i < candidates.Count; i++)
            {
                ITargetCandidate candidate = candidates[i];

                if (candidate == null || !candidate.IsTargetable)
                    continue;

                float sqrDistance = _distance.SqrDistance(context.Origin, candidate.Position);

                if (sqrDistance > context.RangeSquared)
                    continue;

                if (best == null || sqrDistance < bestSqrDistance || (sqrDistance == bestSqrDistance && candidate.EntityId.Value < best.EntityId.Value))
                {
                    best = candidate;
                    bestSqrDistance = sqrDistance;
                }
            }

            return best;
        }
    }
}
