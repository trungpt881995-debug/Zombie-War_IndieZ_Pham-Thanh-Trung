using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Projectile.Events
{
    public readonly struct ProjectileLaunchedEvent : IEvent
    {
        public EntityId ProjectileId { get; }
        public EntityId OwnerId { get; }

        public ProjectileLaunchedEvent(
            EntityId projectileId,
            EntityId ownerId)
        {
            ProjectileId = projectileId;
            OwnerId = ownerId;
        }
    }
}
