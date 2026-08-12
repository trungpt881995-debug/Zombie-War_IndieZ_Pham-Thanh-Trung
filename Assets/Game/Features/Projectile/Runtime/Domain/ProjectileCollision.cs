using System;
using GameplayCore.Damage;
using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileCollision
    {
        public ProjectileCollisionKind Kind { get; }
        public IDamageable Damageable { get; }
        public EntityId TargetId { get; }
        public ProjectilePoint ContactPoint { get; }
        public bool HasDamageable => Damageable != null;

        private ProjectileCollision(ProjectileCollisionKind kind, IDamageable damageable, EntityId targetId, in ProjectilePoint contactPoint)
        {
            Kind = kind;
            Damageable = damageable;
            TargetId = targetId;
            ContactPoint = contactPoint;
        }

        public static ProjectileCollision ForDamageable(IDamageable damageable, in ProjectilePoint contactPoint)
        {
            if (damageable == null) 
            throw new ArgumentNullException(nameof(damageable));

            return new ProjectileCollision(ProjectileCollisionKind.Damageable, damageable, damageable.EntityId, in contactPoint);
        }

        public static ProjectileCollision ForSurface( ProjectileCollisionKind kind, in ProjectilePoint contactPoint)
        {
            if (kind == ProjectileCollisionKind.Damageable)
                throw new ArgumentException("Use ForDamageable for damageable collisions.", nameof(kind));
                
            return new ProjectileCollision(kind, null, default, in contactPoint);
        }
    }
}
