using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Level.Events;

namespace ZombieWar.Integration.Audio.Level
{
    public sealed class LevelAudioBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IAudioRuntime _audio;

        private IDisposable _subscription;

        public LevelAudioBridge(
            IEventSubscriber events,
            IAudioRuntime audio)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public void Start()
        {
            if (_subscription != null)
            {
                return;
            }

            _subscription =
                _events.Subscribe<SoldierGroupLevelChangedEvent>(OnLevelChanged);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnLevelChanged(SoldierGroupLevelChangedEvent evt)
        {
            if (evt.Current == evt.Previous)
            {
                return;
            }

            var request =
                new AudioRequest(AudioId.SoldierGroupLevelUp);

            _audio.Play(in request);
        }
    }
}
