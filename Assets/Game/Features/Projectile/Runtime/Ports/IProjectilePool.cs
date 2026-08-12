using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Ports
{
    public interface IProjectilePool
    {
        ProjectileController Acquire(ProjectilePoolKey key);
        void Release(ProjectilePoolKey key, ProjectileController projectile);
    }
}
