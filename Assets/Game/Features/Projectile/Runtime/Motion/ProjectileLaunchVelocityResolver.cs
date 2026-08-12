using System;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Motion
{
    public sealed class ProjectileLaunchVelocityResolver : IProjectileLaunchVelocityResolver
    {
        private readonly IProjectileLaunchVelocitySolver _linear;
        private readonly IProjectileLaunchVelocitySolver _ballistic;

        public ProjectileLaunchVelocityResolver(IProjectileLaunchVelocitySolver linear, IProjectileLaunchVelocitySolver ballistic)
        {
            _linear = linear ?? throw new ArgumentNullException(nameof(linear));
            _ballistic = ballistic ?? throw new ArgumentNullException(nameof(ballistic));
        }

        public bool TryResolve(in ProjectileLaunchRequest request, out ProjectileVector velocity)
        {
            switch (request.MotionKind)
            {
                case ProjectileMotionKind.Linear:
                    return _linear.TrySolve(in request, out velocity);
                case ProjectileMotionKind.Ballistic:
                    return _ballistic.TrySolve(in request, out velocity);
                default:
                    velocity = ProjectileVector.Zero;
                    return false;
            }
        }
    }
}
