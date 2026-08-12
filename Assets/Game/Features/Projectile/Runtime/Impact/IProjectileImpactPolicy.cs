using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Model;

namespace ZombieWar.Features.Projectile.Impact
{
    public interface IProjectileImpactPolicy
    {
        ProjectileImpactDecision Evaluate(ProjectileModel projectile, in ProjectileCollision collision);
    }
}
