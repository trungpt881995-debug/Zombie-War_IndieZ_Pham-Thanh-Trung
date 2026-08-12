using ZombieWar.Features.Projectile.Motion;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Services;

namespace ZombieWar.Features.Projectile.Factories
{
    public interface IProjectileLauncherFactory
    {
        IProjectileLauncher Create(IProjectilePool pool, IProjectileLaunchVelocityResolver velocityResolver);
    }
}
