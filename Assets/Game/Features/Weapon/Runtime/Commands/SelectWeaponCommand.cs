using GameplayCore.Commands;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Commands
{
    public readonly struct SelectWeaponCommand : IGameplayCommand
    {
        public WeaponType Weapon { get; }
        public SelectWeaponCommand(WeaponType weapon) => Weapon = weapon;
    }
}
