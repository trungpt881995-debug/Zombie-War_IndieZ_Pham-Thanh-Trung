using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Weapon.Services;

namespace ZombieWar.Integration.Weapon
{
    public sealed class WeaponTargetRangeProvider : ITargetRangeProvider
    {
        private readonly IWeaponRuntime _runtime;
        public WeaponTargetRangeProvider(IWeaponRuntime runtime) => _runtime = runtime;
        public float CurrentTargetRange => _runtime != null ? _runtime.CurrentTargetRange : 0f;
    }
}
