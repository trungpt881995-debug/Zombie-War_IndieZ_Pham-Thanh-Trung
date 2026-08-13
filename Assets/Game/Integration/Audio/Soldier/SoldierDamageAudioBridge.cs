using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Health.Events;

namespace ZombieWar.Integration.Audio.Soldier
{
    public interface IAudioSoldierBinding
    {
        bool IsBound { get; }
        EntityId SoldierGroupId { get; }

        void Bind(EntityId soldierGroupId);
        void Unbind(EntityId soldierGroupId);
    }

    public sealed class SoldierDamageAudioBridge :
        IAudioSoldierBinding,
        IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IAudioRuntime _audio;

        private IDisposable _subscription;
        private EntityId _soldierGroupId;
        private bool _bound;

        public SoldierDamageAudioBridge(
            IEventSubscriber events,
            IAudioRuntime audio)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
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

            float damage = evt.PreviousHealth - evt.CurrentHealth;
            float intensity = evt.MaxHealth > 0f
                ? 1f + Math.Min(0.5f, damage / evt.MaxHealth)
                : 1f;

            var request = new AudioRequest(
                AudioId.SoldierDamage,
                intensity: intensity);

            _audio.Play(in request);
        }
    }
}
