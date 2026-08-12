using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Strategies
{
    public interface IWeaponFireStrategy
    {
        void OnTargetAcquired(in WeaponDefinition weapon, in WeaponFireContext context);
        bool Fire(in WeaponDefinition weapon, in WeaponFireContext context);
        void OnTargetCleared(EntityId ownerId);
    }
}
