using System;
using GameplayCore.Entities;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponProjectileRequest
    {
        public EntityId OwnerId { get; }
        public EntityId TargetId { get; }
        public bool HasTargetEntity { get; }
        public WeaponProjectileProfileId Profile { get; }
        public WeaponPoint Origin { get; }
        public WeaponDirection Direction { get; }
        public WeaponPoint TargetPoint { get; }
        public bool HasTargetPoint { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float MaxRange { get; }
        public float MaxLifetime { get; }
        public float ExplosionRadius { get; }

        // Backward-compatible constructor for callers that do not carry an entity target.
        public WeaponProjectileRequest(
            EntityId ownerId,
            WeaponProjectileProfileId profile,
            in WeaponPoint origin,
            in WeaponDirection direction,
            float speed,
            float damage,
            float maxRange,
            float maxLifetime,
            in WeaponPoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius = 0f)
            : this(
                ownerId,
                default(EntityId),
                false,
                profile,
                in origin,
                in direction,
                speed,
                damage,
                maxRange,
                maxLifetime,
                in targetPoint,
                hasTargetPoint,
                explosionRadius)
        {
        }

        // Preferred constructor for targeted Weapon shots.
        public WeaponProjectileRequest(
            EntityId ownerId,
            EntityId targetId,
            WeaponProjectileProfileId profile,
            in WeaponPoint origin,
            in WeaponDirection direction,
            float speed,
            float damage,
            float maxRange,
            float maxLifetime,
            in WeaponPoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius = 0f)
            : this(
                ownerId,
                targetId,
                true,
                profile,
                in origin,
                in direction,
                speed,
                damage,
                maxRange,
                maxLifetime,
                in targetPoint,
                hasTargetPoint,
                explosionRadius)
        {
        }

        private WeaponProjectileRequest(
            EntityId ownerId,
            EntityId targetId,
            bool hasTargetEntity,
            WeaponProjectileProfileId profile,
            in WeaponPoint origin,
            in WeaponDirection direction,
            float speed,
            float damage,
            float maxRange,
            float maxLifetime,
            in WeaponPoint targetPoint,
            bool hasTargetPoint,
            float explosionRadius)
        {
            if (profile == WeaponProjectileProfileId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(profile));
            }

            ValidatePositive(speed, nameof(speed));
            ValidatePositive(damage, nameof(damage));
            ValidatePositive(maxRange, nameof(maxRange));
            ValidatePositive(maxLifetime, nameof(maxLifetime));

            if (float.IsNaN(explosionRadius) ||
                float.IsInfinity(explosionRadius) ||
                explosionRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(explosionRadius));
            }

            OwnerId = ownerId;
            TargetId = targetId;
            HasTargetEntity = hasTargetEntity;
            Profile = profile;
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
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }
}
