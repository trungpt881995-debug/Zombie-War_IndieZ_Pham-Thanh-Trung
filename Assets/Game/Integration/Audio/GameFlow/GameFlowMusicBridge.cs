using System;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.Integration.Audio.GameFlow
{
    public sealed class GameFlowMusicBridge : IDisposable
    {
        private readonly GameFlowModel _model;
        private readonly IAudioRuntime _audio;

        private bool _started;

        public GameFlowMusicBridge(
            GameFlowModel model,
            IAudioRuntime audio)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _model.StateChanged += OnStateChanged;

            Apply(_model.CurrentState);
        }

        public void Dispose()
        {
            if (!_started)
            {
                return;
            }

            _started = false;
            _model.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameFlowStateId state)
        {
            Apply(state);
        }

        private void Apply(GameFlowStateId state)
        {
            switch (state)
            {
                case GameFlowStateId.MainMenu:
                    _audio.PlayMusic(AudioId.MainMenuMusic);
                    break;

                case GameFlowStateId.Gameplay:
                    _audio.PlayMusic(AudioId.GameplayMusic);
                    break;

                case GameFlowStateId.Boot:
                    _audio.StopMusic();
                    break;
            }
        }
    }
}
