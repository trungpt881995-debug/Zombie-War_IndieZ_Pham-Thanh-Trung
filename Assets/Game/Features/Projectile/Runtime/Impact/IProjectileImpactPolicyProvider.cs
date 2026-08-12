using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Impact
{
    public interface IProjectileImpactPolicyProvider
    {
        IProjectileImpactPolicy Get(ProjectileImpactMode mode);
    }
}
