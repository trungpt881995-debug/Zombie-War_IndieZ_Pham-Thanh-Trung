using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Health.Domain;
using ZombieWar.Features.Health.Events;
using ZombieWar.Features.UI.Presentation;

namespace ZombieWar.Integration.UI.Health
{
    public sealed class HealthUIBridge : IUIHealthBinding, IDisposable
    {
        private readonly HealthPresenter _presenter;
        private readonly IEventSubscriber _events;

        private IDisposable _subscription;
        private IReadOnlyHealth _health;

        public bool IsBound => _health != null;
        public EntityId OwnerId { get; private set; }

        public HealthUIBridge(
            HealthPresenter presenter,
            IEventSubscriber events)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Start()
        {
            EnsureStarted();
        }

        public void Bind(
            EntityId ownerId,
            IReadOnlyHealth health)
        {
            if (ownerId.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerId));

            _health = health ?? throw new ArgumentNullException(nameof(health));
            OwnerId = ownerId;

            // Safe regardless of bootstrap-vs-scene startup ordering.
            EnsureStarted();

            // HealthController construction does not publish HealthChangedEvent,
            // so render the initial full-health snapshot immediately.
            _presenter.Present(
                _health.NormalizedHealth,
                _health.CurrentHealth,
                _health.MaxHealth);
        }

        public void Unbind(EntityId ownerId)
        {
            if (!IsBound || !OwnerId.Equals(ownerId))
                return;

            _health = null;
            OwnerId = default;
        }

        private void EnsureStarted()
        {
            if (_subscription != null)
                return;

            _subscription =
                _events.Subscribe<HealthChangedEvent>(OnChanged);
        }

        private void OnChanged(HealthChangedEvent evt)
        {
            if (!IsBound || !evt.OwnerId.Equals(OwnerId))
                return;

            _presenter.Present(
                evt.NormalizedHealth,
                evt.CurrentHealth,
                evt.MaxHealth);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _health = null;
            OwnerId = default;
        }
    }
}
