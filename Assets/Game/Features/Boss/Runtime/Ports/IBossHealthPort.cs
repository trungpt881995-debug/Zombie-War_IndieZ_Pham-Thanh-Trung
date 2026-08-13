using GameplayCore.Damage; using GameplayCore.Entities;
namespace ZombieWar.Features.Boss.Ports { public interface IBossHealthPort { bool IsAlive{get;} float CurrentHealth{get;} float MaxHealth{get;} void Initialize(EntityId entityId,float maxHealth); void ApplyDamage(DamageInfo damage); } }
