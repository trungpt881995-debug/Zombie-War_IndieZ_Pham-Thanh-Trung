using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Services
{
    public interface IWeaponAttackService
    {
        void Update(EntityId ownerId, in WeaponTarget target, float deltaTime);
        void ClearTarget(EntityId ownerId);
        void ClearAll();
    }
}
