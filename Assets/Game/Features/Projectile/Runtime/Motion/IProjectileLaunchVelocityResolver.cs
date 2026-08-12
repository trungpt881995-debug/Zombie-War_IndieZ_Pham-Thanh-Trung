using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Motion
{
    public interface IProjectileLaunchVelocityResolver
    {
        bool TryResolve(in ProjectileLaunchRequest request, out ProjectileVector velocity);
    }
}
