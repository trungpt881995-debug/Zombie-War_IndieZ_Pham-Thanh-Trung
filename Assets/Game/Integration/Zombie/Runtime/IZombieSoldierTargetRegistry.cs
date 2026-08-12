using GameplayCore.Entities;

namespace ZombieWar.Integration.Zombie
{
    public interface IZombieSoldierTargetRegistry
    {
        bool Register(EntityId soldierId, IZombieTargetSource source);
        bool Unregister(EntityId soldierId);
        void Clear();
    }
}
