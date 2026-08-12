using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Events
{
    public readonly struct WeaponCooldownReadyEvent : IEvent
    {
        public WeaponType Weapon { get; }
        public WeaponCooldownReadyEvent(WeaponType weapon) => Weapon = weapon;
    }
}
