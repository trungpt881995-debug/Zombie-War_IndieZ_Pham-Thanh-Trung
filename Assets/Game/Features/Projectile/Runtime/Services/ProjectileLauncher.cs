using System;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Motion;
using ZombieWar.Features.Projectile.Ports;

namespace ZombieWar.Features.Projectile.Services
{
    public sealed class ProjectileLauncher : IProjectileLauncher
    {
        private readonly IEntityIdGenerator _ids;
        private readonly IProjectilePool _pool;
        private readonly IProjectileLaunchVelocityResolver _velocityResolver;

        public ProjectileLauncher(IEntityIdGenerator ids, IProjectilePool pool, IProjectileLaunchVelocityResolver velocityResolver)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _velocityResolver = velocityResolver ?? throw new ArgumentNullException(nameof(velocityResolver));
        }

        public bool TryLaunch(in ProjectileLaunchRequest request, out EntityId projectileId)
        {
            projectileId = default;
            ProjectileController projectile = _pool.Acquire(request.PoolKey);
            if (projectile == null) return false;

            if (!_velocityResolver.TryResolve(in request, out ProjectileVector velocity))
            {
                _pool.Release(request.PoolKey, projectile);
                return false;
            }

            projectileId = _ids.Next();
            projectile.Launch(projectileId, in request, in velocity);
            return true;
        }
    }
}
