using System;
using UnityEngine;
using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Unity.Config
{
    [CreateAssetMenu(fileName = "WeaponCatalogConfig", menuName = "Zombie War/Weapon/Weapon Catalog Config")]
    public sealed class WeaponCatalogConfig : ScriptableObject
    {
        [SerializeField] private WeaponType initialWeapon = WeaponType.Pistol;
        [SerializeField] private WeaponConfig[] weapons = new WeaponConfig[6];

        public WeaponType InitialWeapon => initialWeapon;

        public WeaponCatalog CreateCatalog()
        {
            if (weapons == null || weapons.Length != WeaponCatalog.RequiredWeaponCount)
                throw new InvalidOperationException("WeaponCatalogConfig requires exactly 6 WeaponConfig assets.");
            var definitions = new WeaponDefinition[WeaponCatalog.RequiredWeaponCount];
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] == null)
                    throw new InvalidOperationException($"WeaponConfig at index {i} is not assigned.");
                definitions[i] = weapons[i].CreateDefinition();
            }
            return new WeaponCatalog(definitions);
        }
    }
}
