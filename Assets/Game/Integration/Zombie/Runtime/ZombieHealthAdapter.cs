using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Health.Controller;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieHealthAdapter : IZombieHealthPort
    {
        private readonly IHealthFactory _factory;
        private HealthController _health;
        public ZombieHealthAdapter(IHealthFactory factory) => _factory = factory;
        public bool IsAlive => _health != null && _health.IsAlive;
        public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
        public float MaxHealth => _health != null ? _health.MaxHealth : 0f;
        public void Initialize(EntityId entityId, float maxHealth) => _health = _factory.Create(entityId, maxHealth);
        public void ApplyDamage(DamageInfo damage) => _health?.ApplyDamage(damage);
    }
}
