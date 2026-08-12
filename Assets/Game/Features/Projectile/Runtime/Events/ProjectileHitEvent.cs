using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Events
{
    public readonly struct ProjectileHitEvent : IEvent
    {
        public EntityId ProjectileId { get; }
        public EntityId OwnerId { get; }
        public EntityId TargetId { get; }
        public float Damage { get; }
        public ProjectileHitEvent(EntityId projectileId, EntityId ownerId, EntityId targetId, float damage)
        { 
            ProjectileId = projectileId; 
            OwnerId = ownerId; 
            TargetId = targetId; 
            Damage = damage; 
        }
    }
}
