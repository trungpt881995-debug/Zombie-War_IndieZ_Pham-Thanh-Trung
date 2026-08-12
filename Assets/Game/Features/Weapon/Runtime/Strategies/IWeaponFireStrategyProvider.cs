using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Strategies
{
    public interface IWeaponFireStrategyProvider
    {
        IWeaponFireStrategy Get(WeaponType type);
    }
}
