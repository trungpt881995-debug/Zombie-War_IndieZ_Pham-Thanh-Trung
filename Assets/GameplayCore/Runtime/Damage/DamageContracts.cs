using GameplayCore.Entities;

namespace GameplayCore.Damage
{
    public readonly struct DamageInfo
    {
        public EntityId Source { get; }
        public float Amount { get; }
        public string Type { get; }
        public DamageInfo(EntityId source, float amount, string type = "Default") { Source = source; Amount = amount; Type = type ?? "Default"; }
    }

    public interface IDamageable
    {
        EntityId EntityId { get; }
        bool IsAlive { get; }
        void ApplyDamage(DamageInfo damage);
    }

    public interface IDamageService
    {
        bool TryApply(IDamageable target, DamageInfo damage);
    }

    public sealed class DamageService : IDamageService
    {
        public bool TryApply(IDamageable target, DamageInfo damage)
        {
            if (target == null || !target.IsAlive || damage.Amount <= 0f) return false;
            target.ApplyDamage(damage);
            return true;
        }
    }
}
