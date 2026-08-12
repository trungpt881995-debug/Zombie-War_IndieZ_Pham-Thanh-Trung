using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Events
{
    public readonly struct WeaponCooldownStartedEvent : IEvent
    {
        public WeaponType Weapon { get; }
        public float Duration { get; }
        public WeaponCooldownStartedEvent(WeaponType weapon, float duration)
        { Weapon = weapon; Duration = duration; }
    }
}
