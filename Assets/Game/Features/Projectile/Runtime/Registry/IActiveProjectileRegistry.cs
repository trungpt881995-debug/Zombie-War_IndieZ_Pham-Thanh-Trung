using ZombieWar.Features.Projectile.Controller;

namespace ZombieWar.Features.Projectile.Registry
{
    public interface IActiveProjectileRegistry
    {
        int Count { get; }
        ProjectileController GetAt(int index);
        bool Add(ProjectileController projectile);
        bool Remove(ProjectileController projectile);
        void Clear();
    }
}
