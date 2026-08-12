using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileExplosionRequest
    {
        public EntityId OwnerId { get; }
        public EntityId ProjectileId { get; }
        public ProjectilePoint Center { get; }
        public float Radius { get; }
        public float Damage { get; }

        public ProjectileExplosionRequest(EntityId ownerId, EntityId projectileId, in ProjectilePoint center, float radius, float damage)
        {
            OwnerId = ownerId;
            ProjectileId = projectileId;
            Center = center;
            Radius = radius;
            Damage = damage;
        }
    }
}
