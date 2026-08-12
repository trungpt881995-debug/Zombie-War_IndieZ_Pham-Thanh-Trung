using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public interface IWeaponMuzzleProvider
    {
        bool TryGetMuzzle(EntityId ownerId, out WeaponMuzzle muzzle);
    }
}
