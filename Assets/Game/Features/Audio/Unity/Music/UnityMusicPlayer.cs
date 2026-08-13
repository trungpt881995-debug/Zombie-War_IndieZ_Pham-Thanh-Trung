using System;
using UnityEngine;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Unity.Config;

namespace ZombieWar.Features.Audio.Unity.Music
{
    public sealed class UnityMusicPlayer :
        MonoBehaviour,
        IMusicPlaybackPort
    {
        [SerializeField] private AudioSource sourceA;
        [SerializeField] private AudioSource sourceB;

        private AudioCatalogConfig _catalog;
        private AudioSource _current;
        private AudioSource _incoming;

        private AudioId _currentId = AudioId.None;
        private AudioId _incomingId = AudioId.None;

        private float _fadeDuration;
        private float _fadeElapsed;
        private float _targetVolume = 1f;
        private bool _stopping;

        public AudioId CurrentMusic =>
            _incomingId != AudioId.None
                ? _incomingId
                : _currentId;

        public void Bind(AudioCatalogConfig catalog)
        {
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));

            EnsureSources();
        }

        public bool Play(
            AudioId id,
            float fadeDuration,
            float volume)
        {
            EnsureSources();

            if (_catalog == null ||
                !_catalog.TryGetConfig(
                    id,
                    out AudioConfig config))
            {
                return false;
            }

            AudioClip clip =
                FirstValidClip(config.Clips);

            if (clip == null)
            {
                return false;
            }

            if (CurrentMusic == id &&
                ((_incoming != null &&
                  _incoming.isPlaying) ||
                 (_current != null &&
                  _current.isPlaying)))
            {
                SetVolume(volume);
                return true;
            }

            _targetVolume = Mathf.Clamp01(volume);
            _fadeDuration =
                Mathf.Max(0f, fadeDuration);

            _fadeElapsed = 0f;
            _stopping = false;

            _incoming = OtherSource(_current);

            ConfigureSource(
                _incoming,
                config,
                clip);

            _incoming.volume =
                _fadeDuration > 0f
                    ? 0f
                    : _targetVolume;

            _incoming.Play();
            _incomingId = id;

            if (_fadeDuration <= 0f)
            {
                _current?.Stop();
                PromoteIncoming();
            }

            return true;
        }

        public void Stop(float fadeDuration)
        {
            EnsureSources();

            _incoming?.Stop();
            _incoming = null;
            _incomingId = AudioId.None;

            if (_current == null ||
                !_current.isPlaying)
            {
                Clear();
                return;
            }

            _fadeDuration =
                Mathf.Max(0f, fadeDuration);

            _fadeElapsed = 0f;
            _stopping = true;

            if (_fadeDuration <= 0f)
            {
                Clear();
            }
        }

        public void SetVolume(float volume)
        {
            _targetVolume =
                Mathf.Clamp01(volume);

            if (!_stopping &&
                _incoming == null &&
                _current != null)
            {
                _current.volume =
                    _targetVolume;
            }
        }

        public void Tick(float deltaTime)
        {
            if (_fadeDuration <= 0f)
            {
                return;
            }

            _fadeElapsed +=
                Mathf.Max(0f, deltaTime);

            float normalized =
                Mathf.Clamp01(
                    _fadeElapsed /
                    _fadeDuration);

            if (_stopping)
            {
                if (_current != null)
                {
                    _current.volume =
                        _targetVolume *
                        (1f - normalized);
                }

                if (normalized >= 1f)
                {
                    Clear();
                }

                return;
            }

            if (_incoming != null)
            {
                _incoming.volume =
                    _targetVolume *
                    normalized;
            }

            if (_current != null)
            {
                _current.volume =
                    _targetVolume *
                    (1f - normalized);
            }

            if (normalized >= 1f)
            {
                _current?.Stop();
                PromoteIncoming();
            }
        }

        public void Clear()
        {
            sourceA?.Stop();
            sourceB?.Stop();

            _current = null;
            _incoming = null;
            _currentId = AudioId.None;
            _incomingId = AudioId.None;
            _fadeDuration = 0f;
            _fadeElapsed = 0f;
            _stopping = false;
        }

        private void PromoteIncoming()
        {
            _current = _incoming;
            _currentId = _incomingId;

            _incoming = null;
            _incomingId = AudioId.None;

            _fadeDuration = 0f;
            _fadeElapsed = 0f;
            _stopping = false;

            if (_current != null)
            {
                _current.volume =
                    _targetVolume;
            }
        }

        private AudioSource OtherSource(
            AudioSource source)
        {
            if (source == null ||
                source == sourceB)
            {
                return sourceA;
            }

            return sourceB;
        }

        private void EnsureSources()
        {
            if (sourceA == null)
            {
                sourceA =
                    CreateSource("Music_A");
            }

            if (sourceB == null)
            {
                sourceB =
                    CreateSource("Music_B");
            }
        }

        private AudioSource CreateSource(
            string objectName)
        {
            var child =
                new GameObject(objectName);

            child.transform.SetParent(
                transform,
                false);

            AudioSource source =
                child.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;

            return source;
        }

        private static void ConfigureSource(
            AudioSource source,
            AudioConfig config,
            AudioClip clip)
        {
            source.Stop();
            source.clip = clip;
            source.outputAudioMixerGroup =
                config.OutputMixerGroup;

            source.loop = true;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
        }

        private static AudioClip FirstValidClip(
            AudioClip[] clips)
        {
            if (clips == null)
            {
                return null;
            }

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    return clips[i];
                }
            }

            return null;
        }
    }
}
