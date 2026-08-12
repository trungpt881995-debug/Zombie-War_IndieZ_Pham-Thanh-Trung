using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Integration.Weapon
{
    public interface IWeaponMuzzleSource
    {
        WeaponMuzzle CurrentMuzzle { get; }
    }
}
