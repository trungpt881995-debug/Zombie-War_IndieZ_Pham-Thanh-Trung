using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using ZombieWar.Features.Health.Domain;
using ZombieWar.Features.Health.Events;
using ZombieWar.Features.Health.Model;
using ZombieWar.Features.Health.View;

namespace ZombieWar.Features.Health.Controller
{
    /// <summary>
    /// MVC Controller / application orchestrator for Health.
    /// - delegates HP rules to HealthModel
    /// - renders through IHealthView
    /// - publishes cross-feature notifications through IEventBus
    /// - implements Gameplay Core IDamageable without exposing concrete HealthModel
    /// </summary>
    public sealed class HealthController :
        IController,
        IReadOnlyHealth,
        IHealthDamageReceiver,
        IHealthResettable
    {
        private readonly EntityId _ownerId;
        private readonly HealthModel _model;
        private readonly IHealthView _view;
        private readonly IEventBus _eventBus;

        public EntityId EntityId => _ownerId;
        public float CurrentHealth => _model.CurrentHealth;
        public float MaxHealth => _model.MaxHealth;
        public float NormalizedHealth => _model.NormalizedHealth;
        public bool IsAlive => _model.IsAlive;
        public bool IsDepleted => _model.IsDepleted;
        public HealthState State => _model.State;

        public HealthController(
            EntityId ownerId,
            HealthModel model,
            IHealthView view,
            IEventBus eventBus)
        {
            _ownerId = ownerId;
            _model = model ?? throw new System.ArgumentNullException(nameof(model));
            _view = view ?? NullHealthView.Instance;
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));

            Render();
        }

        public void ApplyDamage(DamageInfo damage)
        {
            ApplyDamage(damage.Amount);
        }

        public void ApplyDamage(float amount)
        {
            var result = _model.Reduce(amount);
            if (!result.Changed)
            {
                return;
            }

            Render();
            PublishChanged(result);

            if (result.BecameDepleted)
            {
                _eventBus.Publish(new HealthDepletedEvent(_ownerId));
            }
        }

        public void ResetHealth()
        {
            var result = _model.Reset();
            Render();

            if (result.Changed)
            {
                PublishChanged(result);
            }
        }

        private void PublishChanged(in HealthChangeResult result)
        {
            _eventBus.Publish(
                new HealthChangedEvent(
                    _ownerId,
                    result.PreviousHealth,
                    result.CurrentHealth,
                    _model.MaxHealth));
        }

        private void Render()
        {
            var state = new HealthViewState(
                _model.CurrentHealth,
                _model.MaxHealth,
                _model.NormalizedHealth,
                _model.IsAlive,
                _model.State);

            _view.Render(in state);
        }
    }
}
