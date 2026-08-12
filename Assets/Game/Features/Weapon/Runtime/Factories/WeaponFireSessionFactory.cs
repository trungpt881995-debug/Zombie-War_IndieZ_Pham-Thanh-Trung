using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Model;

namespace ZombieWar.Features.Weapon.Factories
{
    public sealed class WeaponFireSessionFactory : IWeaponFireSessionFactory
    {
        public WeaponFireSessionModel Create(EntityId ownerId) =>
            new WeaponFireSessionModel(ownerId);
    }
}
