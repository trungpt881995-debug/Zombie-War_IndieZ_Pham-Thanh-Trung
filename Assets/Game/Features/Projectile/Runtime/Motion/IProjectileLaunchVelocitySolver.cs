using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Motion
{
    public interface IProjectileLaunchVelocitySolver
    {
        ProjectileMotionKind Kind { get; }
        bool TrySolve(in ProjectileLaunchRequest request, out ProjectileVector velocity);
    }
}
