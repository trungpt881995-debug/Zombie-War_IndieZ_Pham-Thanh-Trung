using ZombieWar.Features.Health.Domain;

namespace ZombieWar.Features.Health.View
{
    public readonly struct HealthViewState
    {
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float NormalizedHealth { get; }
        public bool IsAlive { get; }
        public HealthState State { get; }

        public HealthViewState(
            float currentHealth,
            float maxHealth,
            float normalizedHealth,
            bool isAlive,
            HealthState state)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            NormalizedHealth = normalizedHealth;
            IsAlive = isAlive;
            State = state;
        }
    }
}
