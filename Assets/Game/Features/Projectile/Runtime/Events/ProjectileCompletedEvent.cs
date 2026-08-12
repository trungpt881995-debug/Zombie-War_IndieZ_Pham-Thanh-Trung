using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Events
{
    public readonly struct ProjectileCompletedEvent : IEvent
    {
        public EntityId ProjectileId { get; }
        public EntityId OwnerId { get; }
        public ProjectileEndReason Reason { get; }
        public ProjectileCompletedEvent(EntityId projectileId, EntityId ownerId, ProjectileEndReason reason)
        { 
            ProjectileId = projectileId; 
            OwnerId = ownerId; 
            Reason = reason; 
        }
    }
}
