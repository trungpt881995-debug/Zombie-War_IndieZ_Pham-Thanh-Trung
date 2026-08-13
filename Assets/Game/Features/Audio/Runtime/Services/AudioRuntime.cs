using System;
using ZombieWar.Features.Audio.Catalog;
using ZombieWar.Features.Audio.Controller;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Model;
using ZombieWar.Features.Audio.Ports;

namespace ZombieWar.Features.Audio.Services
{
    public interface IAudioRuntime
    {
        bool IsInitialized { get; }
        WorldAudioMode WorldMode { get; }
        int ActiveVoiceCount { get; }
        AudioId CurrentMusic { get; }

        AudioPlayResult Play(in AudioRequest request);
        bool IsPlaying(AudioHandle handle);
        bool Stop(AudioHandle handle);
        void SetWorldMode(WorldAudioMode mode);

        bool PlayMusic(
            AudioId id,
            float fadeDuration = 0.35f);

        void StopMusic(float fadeDuration = 0.35f);
        void CancelAll();
    }

    public interface IAudioRuntimeConfigurator
    {
        void Initialize(
            IAudioCatalog catalog,
            IAudioVoicePool pool,
            IMusicPlaybackPort music);

        void Shutdown();
    }

    public interface IAudioRuntimeDriver
    {
        void Tick(float deltaTime);
    }

    public sealed class AudioRuntime :
        IAudioRuntime,
        IAudioRuntimeConfigurator,
        IAudioRuntimeDriver
    {
        private readonly AudioModel _model;
        private readonly AudioController _audio;
        private readonly MusicController _music;

        private WorldAudioMode _desiredWorldMode = WorldAudioMode.Inactive;
        private AudioId _desiredMusic = AudioId.None;
        private float _desiredMusicFade = 0.35f;
        private bool _musicRequested;

        public AudioRuntime(
            AudioModel model,
            AudioController audio,
            MusicController music)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _music = music ?? throw new ArgumentNullException(nameof(music));
        }

        public bool IsInitialized => _audio.IsInitialized;
        public WorldAudioMode WorldMode => _model.WorldMode;
        public int ActiveVoiceCount => _model.ActiveVoiceCount;
        public AudioId CurrentMusic => _music.CurrentMusic;

        public AudioPlayResult Play(in AudioRequest request)
        {
            return _audio.Play(in request);
        }

        public bool IsPlaying(AudioHandle handle)
        {
            return _audio.IsPlaying(handle);
        }

        public bool Stop(AudioHandle handle)
        {
            return _audio.Stop(handle);
        }

        public void SetWorldMode(WorldAudioMode mode)
        {
            _desiredWorldMode = mode;

            if (IsInitialized)
            {
                _audio.SetWorldMode(mode);
            }
        }

        public bool PlayMusic(
            AudioId id,
            float fadeDuration = 0.35f)
        {
            _desiredMusic = id;
            _desiredMusicFade = fadeDuration < 0f ? 0f : fadeDuration;
            _musicRequested = id != AudioId.None;

            if (!IsInitialized)
            {
                return _musicRequested;
            }

            return _music.Play(
                id,
                _desiredMusicFade);
        }

        public void StopMusic(float fadeDuration = 0.35f)
        {
            _desiredMusic = AudioId.None;
            _desiredMusicFade = fadeDuration < 0f ? 0f : fadeDuration;
            _musicRequested = false;

            if (IsInitialized)
            {
                _music.Stop(_desiredMusicFade);
            }
        }

        public void CancelAll()
        {
            _desiredMusic = AudioId.None;
            _musicRequested = false;

            if (!IsInitialized)
            {
                return;
            }

            _audio.CancelAll(AudioReleaseReason.Cancelled);
            _music.Stop(0f);
        }

        public void Initialize(
            IAudioCatalog catalog,
            IAudioVoicePool pool,
            IMusicPlaybackPort music)
        {
            _audio.Initialize(
                catalog,
                pool);

            _music.Initialize(music);
            _audio.SetWorldMode(_desiredWorldMode);

            if (_musicRequested)
            {
                _music.Play(
                    _desiredMusic,
                    _desiredMusicFade);
            }
        }

        public void Shutdown()
        {
            _audio.Shutdown();
            _music.Shutdown();
        }

        public void Tick(float deltaTime)
        {
            if (!IsInitialized)
            {
                return;
            }

            _audio.Tick(deltaTime);
            _music.Tick(deltaTime);
        }
    }
}
