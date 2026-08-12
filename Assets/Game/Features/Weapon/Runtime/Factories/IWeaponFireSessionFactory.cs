using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Model;

namespace ZombieWar.Features.Weapon.Factories
{
    public interface IWeaponFireSessionFactory
    {
        WeaponFireSessionModel Create(EntityId ownerId);
    }
}
