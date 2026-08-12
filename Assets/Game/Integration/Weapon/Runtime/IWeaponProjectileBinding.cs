using ZombieWar.Features.Projectile.Services;

namespace ZombieWar.Integration.Weapon
{
    public interface IWeaponProjectileBinding
    {
        bool IsBound { get; }
        void Bind(IProjectileLauncher launcher, in WeaponProjectilePoolMapping mapping);
        void Unbind();
    }
}
