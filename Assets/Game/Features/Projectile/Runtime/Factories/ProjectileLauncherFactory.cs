using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Motion;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Services;

namespace ZombieWar.Features.Projectile.Factories
{
    public sealed class ProjectileLauncherFactory : IProjectileLauncherFactory
    {
        private readonly IEntityIdGenerator _ids;
        public ProjectileLauncherFactory(IEntityIdGenerator ids) => _ids = ids;

        public IProjectileLauncher Create(IProjectilePool pool, IProjectileLaunchVelocityResolver velocityResolver)
        {
            return new ProjectileLauncher(_ids, pool, velocityResolver);
        }
    }
}
