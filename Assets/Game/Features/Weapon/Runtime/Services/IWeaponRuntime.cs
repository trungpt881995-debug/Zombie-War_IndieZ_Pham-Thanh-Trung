using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Services
{
    public interface IWeaponRuntime
    {
        bool IsInitialized { get; }
        bool GameplayEnabled { get; }
        WeaponType CurrentWeapon { get; }
        float CurrentTargetRange { get; }
        WeaponCooldownSnapshot Cooldowns { get; }

        void Initialize(IWeaponCatalog catalog, WeaponType initialWeapon);
        WeaponSelectionResult TrySelect(WeaponType type);
        bool TryGetCurrentDefinition(out WeaponDefinition definition);
        bool TryGetDefinition(WeaponType type, out WeaponDefinition definition);
        void Tick(float deltaTime);
        void SetGameplayEnabled(bool enabled);
        void ResetForGameLevel();
    }
}
