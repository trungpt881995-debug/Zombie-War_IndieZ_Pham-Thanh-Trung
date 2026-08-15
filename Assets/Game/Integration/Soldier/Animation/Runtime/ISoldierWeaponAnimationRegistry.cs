using GameplayCore.Entities;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Integration.Soldier.Animation
{
    public interface ISoldierWeaponAnimationRegistry
    {
        void Register(
            EntityId ownerId,
            ISoldierWeaponAnimationView view);

        void Unregister(EntityId ownerId);

        bool TryGet(
            EntityId ownerId,
            out ISoldierWeaponAnimationView view);

        void Clear();
    }
}
