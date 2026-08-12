using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Motion
{
    public sealed class LinearLaunchVelocitySolver : IProjectileLaunchVelocitySolver
    {
        public ProjectileMotionKind Kind => ProjectileMotionKind.Linear;

        public bool TrySolve(in ProjectileLaunchRequest request, out ProjectileVector velocity)
        {
            velocity = new ProjectileVector(
                request.Direction.X * request.Speed,
                request.Direction.Y * request.Speed,
                request.Direction.Z * request.Speed
                );
                
            return true;
        }
    }
}
