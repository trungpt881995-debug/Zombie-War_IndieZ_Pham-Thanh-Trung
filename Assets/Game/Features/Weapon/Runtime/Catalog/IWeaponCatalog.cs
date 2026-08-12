using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Catalog
{
    public interface IWeaponCatalog
    {
        int Count { get; }
        WeaponDefinition Get(WeaponType type);
        bool TryGet(WeaponType type, out WeaponDefinition definition);
    }
}
