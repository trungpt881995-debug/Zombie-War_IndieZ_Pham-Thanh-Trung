using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Services;

namespace ZombieWar.Integration.Audio.GameState
{
    public sealed class GameStateAudioBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IAudioRuntime _audio;
        private readonly IGameStateRuntime _gameState;

        private IDisposable _subscription;

        public GameStateAudioBridge(
            IEventSubscriber events,
            IAudioRuntime audio,
            IGameStateRuntime gameState)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public void Start()
        {
            if (_subscription != null)
            {
                return;
            }

            ApplyMode(_gameState.State);

            _subscription =
                _events.Subscribe<GameplayStateChangedEvent>(OnStateChanged);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnStateChanged(GameplayStateChangedEvent evt)
        {
            ApplyMode(evt.Current);

            switch (evt.Current)
            {
                case GameplayStateId.GameOver:
                    PlayTerminal(AudioId.GameOver);
                    break;

                case GameplayStateId.LevelComplete:
                    PlayTerminal(AudioId.LevelComplete);
                    break;

                case GameplayStateId.EndGame:
                    PlayTerminal(AudioId.EndGame);
                    break;
            }
        }

        private void ApplyMode(GameplayStateId state)
        {
            switch (state)
            {
                case GameplayStateId.Playing:
                    _audio.SetWorldMode(WorldAudioMode.Playing);
                    break;

                case GameplayStateId.Paused:
                    _audio.SetWorldMode(WorldAudioMode.Suspended);
                    break;

                case GameplayStateId.GameOver:
                case GameplayStateId.LevelComplete:
                case GameplayStateId.EndGame:
                    _audio.SetWorldMode(WorldAudioMode.TerminalDrain);
                    break;

                case GameplayStateId.Inactive:
                default:
                    _audio.SetWorldMode(WorldAudioMode.Inactive);
                    break;
            }
        }

        private void PlayTerminal(AudioId id)
        {
            var request = new AudioRequest(id);
            _audio.Play(in request);
        }
    }
}
