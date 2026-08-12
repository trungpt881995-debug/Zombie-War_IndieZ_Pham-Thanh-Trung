using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Registry;

namespace ZombieWar.Features.Projectile.Factories
{
    public interface IProjectileControllerFactory
    {
        ProjectileController Create(IProjectileView view, IProjectilePool pool, IActiveProjectileRegistry registry);
    }
}
