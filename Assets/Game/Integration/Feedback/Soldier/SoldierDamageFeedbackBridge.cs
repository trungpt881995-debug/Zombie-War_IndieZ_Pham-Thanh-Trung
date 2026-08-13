using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Health.Events;

namespace ZombieWar.Integration.Feedback.Soldier
{
    public interface IFeedbackSoldierBinding
    {
        bool IsBound { get; }
        EntityId SoldierGroupId { get; }

        void Bind(EntityId soldierGroupId);
        void Unbind(EntityId soldierGroupId);
    }

    public sealed class SoldierDamageFeedbackBridge :
        IFeedbackSoldierBinding,
        IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IFeedbackRuntime _feedback;

        private IDisposable _subscription;
        private EntityId _soldierGroupId;
        private bool _bound;

        public SoldierDamageFeedbackBridge(
            IEventSubscriber events,
            IFeedbackRuntime feedback)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        public bool IsBound => _bound;
        public EntityId SoldierGroupId => _soldierGroupId;

        public void Start()
        {
            if (_subscription != null)
            {
                return;
            }

            _subscription =
                _events.Subscribe<HealthChangedEvent>(OnHealthChanged);
        }

        public void Bind(EntityId soldierGroupId)
        {
            _soldierGroupId = soldierGroupId;
            _bound = true;
        }

        public void Unbind(EntityId soldierGroupId)
        {
            if (!_bound || _soldierGroupId != soldierGroupId)
            {
                return;
            }

            _soldierGroupId = default;
            _bound = false;
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;

            _soldierGroupId = default;
            _bound = false;
        }

        private void OnHealthChanged(HealthChangedEvent evt)
        {
            if (!_bound ||
                evt.OwnerId != _soldierGroupId ||
                evt.CurrentHealth >= evt.PreviousHealth)
            {
                return;
            }

            FeedbackId id = evt.NormalizedHealth <= 0.25f
                ? FeedbackId.SoldierCriticalDamage
                : FeedbackId.SoldierDamaged;

            float delta =
                evt.PreviousHealth - evt.CurrentHealth;

            float normalizedDamage = evt.MaxHealth > 0f
                ? delta / evt.MaxHealth
                : 0f;

            float intensity =
                1f + Math.Min(0.5f, normalizedDamage);

            var request = new FeedbackRequest(
                id,
                intensity,
                evt.OwnerId.Value);

            _feedback.Play(in request);
        }
    }
}
