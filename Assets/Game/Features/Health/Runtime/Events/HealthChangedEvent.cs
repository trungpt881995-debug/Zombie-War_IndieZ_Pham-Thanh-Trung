using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Health.Events
{
    public readonly struct HealthChangedEvent : IEvent
    {
        public EntityId OwnerId { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float NormalizedHealth { get; }

        public HealthChangedEvent(
            EntityId ownerId,
            float previousHealth,
            float currentHealth,
            float maxHealth)
        {
            OwnerId = ownerId;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            NormalizedHealth = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        }
    }
}
