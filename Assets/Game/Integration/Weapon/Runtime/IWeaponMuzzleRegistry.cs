using GameplayCore.Entities;

namespace ZombieWar.Integration.Weapon
{
    public interface IWeaponMuzzleRegistry
    {
        void Register(EntityId ownerId, IWeaponMuzzleSource source);
        void Unregister(EntityId ownerId);
        void Clear();
    }
}
