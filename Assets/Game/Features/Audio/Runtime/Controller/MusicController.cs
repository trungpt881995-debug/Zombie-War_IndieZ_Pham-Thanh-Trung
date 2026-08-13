using System;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;

namespace ZombieWar.Features.Audio.Controller
{
    public sealed class MusicController
    {
        private readonly IAudioPreferences _preferences;
        private IMusicPlaybackPort _playback;

        public MusicController(IAudioPreferences preferences)
        {
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        }

        public AudioId CurrentMusic =>
            _playback != null
                ? _playback.CurrentMusic
                : AudioId.None;

        public void Initialize(IMusicPlaybackPort playback)
        {
            _playback = playback ?? throw new ArgumentNullException(nameof(playback));
            ApplyVolume();
        }

        public bool Play(
            AudioId id,
            float fadeDuration)
        {
            if (_playback == null || id == AudioId.None)
            {
                return false;
            }

            return _playback.Play(
                id,
                fadeDuration,
                CurrentVolume());
        }

        public void Stop(float fadeDuration)
        {
            _playback?.Stop(fadeDuration);
        }

        public void Tick(float deltaTime)
        {
            if (_playback == null)
            {
                return;
            }

            ApplyVolume();
            _playback.Tick(deltaTime);
        }

        public void Shutdown()
        {
            _playback?.Clear();
            _playback = null;
        }

        private void ApplyVolume()
        {
            _playback?.SetVolume(CurrentVolume());
        }

        private float CurrentVolume()
        {
            return _preferences.GetCategoryVolume(AudioCategory.Music);
        }
    }
}
