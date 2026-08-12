using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public interface IWeaponProjectilePort
    {
        bool TryLaunch(in WeaponProjectileRequest request);
    }
}
