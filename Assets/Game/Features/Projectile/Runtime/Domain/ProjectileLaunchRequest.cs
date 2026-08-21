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
        public EntityId TargetId { get; }
        public bool HasTargetEntity { get; }
        public ProjectileImpactMode ImpactMode { get; }
        public ProjectilePoint Origin { get; }
        public ProjectileDirection Direction { get; }
        public ProjectilePoint TargetPoint { get; }
        public bool HasTargetPoint { get; }
        public float Damage { get; }
        public float MaxRange { get; }
        public float ExplosionRadius { get; }

        // Backward-compatible constructor for non-targeted Projectile callers.
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
            : this(
                ownerId,
                default(EntityId),
                false,
                impactMode,
                in origin,
                in direction,
                damage,
                maxRange,
                in targetPoint,
                hasTargetPoint,
                explosionRadius)
        {
        }

        // Preferred constructor when the upstream Weapon knows its target entity.
        public ProjectileLaunchRequest(
            EntityId ownerId,
            EntityId targetId,
            ProjectileImpactMode impactMode,
            in ProjectilePoint origin,
            in ProjectileDirection direction,
            float damage,
            float maxRange,
            in ProjectilePoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius = 0f)
            : this(
                ownerId,
                targetId,
                true,
                impactMode,
                in origin,
                in direction,
                damage,
                maxRange,
                in targetPoint,
                hasTargetPoint,
                explosionRadius)
        {
        }

        private ProjectileLaunchRequest(
            EntityId ownerId,
            EntityId targetId,
            bool hasTargetEntity,
            ProjectileImpactMode impactMode,
            in ProjectilePoint origin,
            in ProjectileDirection direction,
            float damage,
            float maxRange,
            in ProjectilePoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius)
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
            TargetId = targetId;
            HasTargetEntity = hasTargetEntity;
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
