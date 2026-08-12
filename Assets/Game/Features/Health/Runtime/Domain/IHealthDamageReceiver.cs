using GameplayCore.Damage;

namespace ZombieWar.Features.Health.Domain
{
    /// <summary>
    /// Health-facing damage contract. It also satisfies Gameplay Core's generic
    /// IDamageable contract so the future Damage Feature does not depend on
    /// HealthController directly.
    /// </summary>
    public interface IHealthDamageReceiver : IDamageable
    {
        void ApplyDamage(float amount);
    }
}
