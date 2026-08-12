using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.View
{
    public readonly struct WeaponViewState
    {
        public WeaponType CurrentWeapon { get; }
        public bool GameplayEnabled { get; }

        public WeaponViewState(WeaponType currentWeapon, bool gameplayEnabled)
        {
            CurrentWeapon = currentWeapon;
            GameplayEnabled = gameplayEnabled;
        }
    }
}
