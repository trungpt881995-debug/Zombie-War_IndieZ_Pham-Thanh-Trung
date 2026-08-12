using GameplayCore.Damage;
using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieHealthPort
    {
        bool IsAlive { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        void Initialize(EntityId entityId, float maxHealth);
        void ApplyDamage(DamageInfo damage);
    }
}
