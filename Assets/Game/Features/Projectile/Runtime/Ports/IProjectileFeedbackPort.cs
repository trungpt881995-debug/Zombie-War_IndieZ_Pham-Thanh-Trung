using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public interface IProjectileFeedbackPort
    {
        void OnHit(EntityId projectileId, EntityId targetId, in ProjectilePoint point);
        void OnExplosion(EntityId projectileId, in ProjectilePoint point, float radius);
    }
}
