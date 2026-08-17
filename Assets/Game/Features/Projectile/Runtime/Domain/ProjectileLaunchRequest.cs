using System;
using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Domain
{
    /// <summary>
    /// Immutable request for immediate hitscan resolution.
    /// Physical-flight data (pool, motion, speed, lifetime) intentionally does not belong here.
    /// </summary>
    public readonly struct ProjectileLaunchRequest
    {
        public EntityId OwnerId { get; }
        public ProjectileImpactMode ImpactMode { get; }
        public ProjectilePoint Origin { get; }
        public ProjectileDirection Direction { get; }
        public ProjectilePoint TargetPoint { get; }
        public bool HasTargetPoint { get; }
        public float Damage { get; }
        public float MaxRange { get; }
        public float ExplosionRadius { get; }

        public ProjectileLaunchRequest(
            EntityId ownerId,
            ProjectileImpactMode impactMode,
            in ProjectilePoint origin,
            in ProjectileDirection direction,
            float damage,
            float maxRange,
            in ProjectilePoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius = 0f)
        {
            ValidatePositive(damage, nameof(damage));
            ValidatePositive(maxRange, nameof(maxRange));

            if (float.IsNaN(explosionRadius) ||
                float.IsInfinity(explosionRadius) ||
                explosionRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(explosionRadius));
            }

            if (impactMode == ProjectileImpactMode.ExplodeOnGround &&
                explosionRadius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(explosionRadius));
            }

            OwnerId = ownerId;
            ImpactMode = impactMode;
            Origin = origin;
            Direction = direction;
            Damage = damage;
            MaxRange = maxRange;
            TargetPoint = targetPoint;
            HasTargetPoint = hasTargetPoint;
            ExplosionRadius = explosionRadius;
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}
