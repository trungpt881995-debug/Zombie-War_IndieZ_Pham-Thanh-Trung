namespace ZombieWar.Features.Health.Domain
{
    public readonly struct HealthChangeResult
    {
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float AppliedAmount { get; }
        public bool Changed { get; }
        public bool BecameDepleted { get; }

        public HealthChangeResult(float previousHealth, float currentHealth, float appliedAmount, bool changed, bool becameDepleted)
        {
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            AppliedAmount = appliedAmount;
            Changed = changed;
            BecameDepleted = becameDepleted;
        }

        public static HealthChangeResult NoChange(float currentHealth)
        {
            return new HealthChangeResult(currentHealth, currentHealth, 0f, false, false);
        }
    }
}
