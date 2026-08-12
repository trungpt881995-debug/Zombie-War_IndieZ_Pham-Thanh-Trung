using System;
using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileLaunchRequest
    {
        public EntityId OwnerId { get; }
        public ProjectilePoolKey PoolKey { get; }
        public ProjectileMotionKind MotionKind { get; }
        public ProjectileImpactMode ImpactMode { get; }
        public ProjectilePoint Origin { get; }
        public ProjectileDirection Direction { get; }
        public ProjectilePoint TargetPoint { get; }
        public bool HasTargetPoint { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float MaxRange { get; }
        public float MaxLifetime { get; }
        public float ExplosionRadius { get; }

        public ProjectileLaunchRequest(
            EntityId ownerId,
            ProjectilePoolKey poolKey,
            ProjectileMotionKind motionKind,
            ProjectileImpactMode impactMode,
            in ProjectilePoint origin,
            in ProjectileDirection direction,
            float speed,
            float damage,
            float maxRange,
            float maxLifetime,
            in ProjectilePoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius = 0f)
        {
            ValidatePositive(speed, nameof(speed));
            ValidatePositive(damage, nameof(damage));
            ValidatePositive(maxRange, nameof(maxRange));
            ValidatePositive(maxLifetime, nameof(maxLifetime));
            if (float.IsNaN(explosionRadius) || float.IsInfinity(explosionRadius) || explosionRadius < 0f)
                throw new ArgumentOutOfRangeException(nameof(explosionRadius));
            if (motionKind == ProjectileMotionKind.Ballistic && !hasTargetPoint)
                throw new ArgumentException("Ballistic projectile requires a target point.", nameof(hasTargetPoint));
            if (impactMode == ProjectileImpactMode.ExplodeOnGround && explosionRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(explosionRadius));

            OwnerId = ownerId;
            PoolKey = poolKey;
            MotionKind = motionKind;
            ImpactMode = impactMode;
            Origin = origin;
            Direction = direction;
            Speed = speed;
            Damage = damage;
            MaxRange = maxRange;
            MaxLifetime = maxLifetime;
            TargetPoint = targetPoint;
            HasTargetPoint = hasTargetPoint;
            ExplosionRadius = explosionRadius;
        }

        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
