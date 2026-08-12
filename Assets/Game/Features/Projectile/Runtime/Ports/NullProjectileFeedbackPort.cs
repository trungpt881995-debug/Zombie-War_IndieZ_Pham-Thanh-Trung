using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public sealed class NullProjectileFeedbackPort : IProjectileFeedbackPort
    {
        public static readonly NullProjectileFeedbackPort Instance = new NullProjectileFeedbackPort();
        private NullProjectileFeedbackPort() { }
        public void OnHit(EntityId projectileId, EntityId targetId, in ProjectilePoint point) { }
        public void OnExplosion(EntityId projectileId, in ProjectilePoint point, float radius) { }
    }
}
