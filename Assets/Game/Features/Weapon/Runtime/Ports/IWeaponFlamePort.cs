using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public interface IWeaponFlamePort
    {
        void Begin(in WeaponFlameRequest request);
        void ApplyTick(in WeaponFlameRequest request);
        void End(EntityId ownerId);
    }
}
