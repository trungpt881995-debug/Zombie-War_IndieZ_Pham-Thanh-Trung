using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public sealed class NullWeaponFlamePort : IWeaponFlamePort
    {
        public static readonly NullWeaponFlamePort Instance = new NullWeaponFlamePort();
        private NullWeaponFlamePort() { }
        public void Begin(in WeaponFlameRequest request) { }
        public void ApplyTick(in WeaponFlameRequest request) { }
        public void End(EntityId ownerId) { }
    }
}
