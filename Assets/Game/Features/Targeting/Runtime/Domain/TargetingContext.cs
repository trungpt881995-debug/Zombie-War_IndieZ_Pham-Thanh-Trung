using System;

namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Per-evaluation data supplied by Soldier/Weapon composition.
    /// TargetRange stays owned by WeaponConfig, not by Targeting.
    /// </summary>
    public readonly struct TargetingContext
    {
        public TargetPoint Origin { get; }
        public float Range { get; }
        public float RangeSquared { get; }

        public TargetingContext(in TargetPoint origin, float range)
        {
            if (float.IsNaN(range) || float.IsInfinity(range) || range < 0f)
                throw new ArgumentOutOfRangeException(nameof(range), "Target range must be finite and >= 0.");

            Origin = origin;
            Range = range;
            RangeSquared = range * range;
        }
    }
}
