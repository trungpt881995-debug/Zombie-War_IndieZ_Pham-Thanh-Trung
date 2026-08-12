using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Events
{
    public readonly struct WeaponSelectedEvent : IEvent
    {
        public WeaponType Previous { get; }
        public WeaponType Current { get; }
        public WeaponSelectedEvent(WeaponType previous, WeaponType current)
        { Previous = previous; Current = current; }
    }
}
