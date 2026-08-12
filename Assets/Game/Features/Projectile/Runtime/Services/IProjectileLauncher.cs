using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Features.Projectile.Services
{
    public interface IProjectileLauncher
    {
        bool TryLaunch(in ProjectileLaunchRequest request, out EntityId projectileId);
    }
}
