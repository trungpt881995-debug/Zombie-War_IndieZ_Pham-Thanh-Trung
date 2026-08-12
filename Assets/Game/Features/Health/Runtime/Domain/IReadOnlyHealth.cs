namespace ZombieWar.Features.Health.Domain
{
    public interface IReadOnlyHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float NormalizedHealth { get; }
        bool IsAlive { get; }
        bool IsDepleted { get; }
        HealthState State { get; }
    }
}
