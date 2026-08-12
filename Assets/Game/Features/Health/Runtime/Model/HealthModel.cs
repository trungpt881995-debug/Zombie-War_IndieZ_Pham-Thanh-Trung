using System;
using ZombieWar.Features.Health.Domain;

namespace ZombieWar.Features.Health.Model
{
    /// <summary>
    /// Pure C# health domain model. No Unity, UI, audio, VFX, GameState or DI code.
    /// Maintains the invariant: 0 <= CurrentHealth <= MaxHealth and MaxHealth > 0.
    /// </summary>
    public sealed class HealthModel : IReadOnlyHealth
    {
        private readonly float _maxHealth;
        private float _currentHealth;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float NormalizedHealth => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
        public bool IsAlive => _currentHealth > 0f;
        public bool IsDepleted => !IsAlive;
        public HealthState State => IsAlive ? HealthState.Alive : HealthState.Depleted;

        public HealthModel(float maxHealth)
        {
            if (float.IsNaN(maxHealth) || float.IsInfinity(maxHealth) || maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealth),
                    maxHealth,
                    "Max health must be a finite value greater than zero.");
            }

            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public HealthChangeResult Reduce(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f || IsDepleted)
            {
                return HealthChangeResult.NoChange(_currentHealth);
            }

            var previousHealth = _currentHealth;
            var nextHealth = previousHealth - amount;
            _currentHealth = nextHealth > 0f ? nextHealth : 0f;

            var appliedAmount = previousHealth - _currentHealth;
            var becameDepleted = previousHealth > 0f && _currentHealth <= 0f;

            return new HealthChangeResult(
                previousHealth,
                _currentHealth,
                appliedAmount,
                appliedAmount > 0f,
                becameDepleted);
        }

        public HealthChangeResult Reset()
        {
            if (_currentHealth == _maxHealth)
            {
                return HealthChangeResult.NoChange(_currentHealth);
            }

            var previousHealth = _currentHealth;
            _currentHealth = _maxHealth;

            return new HealthChangeResult(
                previousHealth,
                _currentHealth,
                _currentHealth - previousHealth,
                true,
                false);
        }
    }
}
