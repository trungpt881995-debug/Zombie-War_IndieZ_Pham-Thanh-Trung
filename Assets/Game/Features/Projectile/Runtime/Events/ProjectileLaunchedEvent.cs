using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Events
{
    public readonly struct ProjectileLaunchedEvent : IEvent
    {
        public EntityId ProjectileId { get; }
        public EntityId OwnerId { get; }
        public ProjectilePoolKey PoolKey { get; }
        public ProjectileLaunchedEvent(EntityId projectileId, EntityId ownerId, ProjectilePoolKey poolKey)
        { 
            ProjectileId = projectileId; 
            OwnerId = ownerId; 
            PoolKey = poolKey; 
        }
    }
}
