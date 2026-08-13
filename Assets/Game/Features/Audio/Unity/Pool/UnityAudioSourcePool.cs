using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Audio.Unity.Config;

namespace ZombieWar.Features.Audio.Unity.Pool
{
    public sealed class UnityAudioSourcePool :
        MonoBehaviour,
        IAudioVoicePool
    {
        [SerializeField, Min(0)] private int prewarmCount = 24;
        [SerializeField, Min(1)] private int maxCapacity = 64;
        [SerializeField] private bool allowGrowth = true;
        [SerializeField] private Transform poolRoot;

        private readonly Stack<Item> _available =
            new Stack<Item>();

        private readonly List<Item> _all =
            new List<Item>();

        private AudioCatalogConfig _catalog;
        private bool _bound;

        public int Capacity => _all.Count;
        public int AvailableCount => _available.Count;

        public void Bind(
            AudioCatalogConfig catalog,
            IAudioPreferences preferences)
        {
            _catalog = catalog ??
                throw new ArgumentNullException(nameof(catalog));

            if (preferences == null)
            {
                throw new ArgumentNullException(nameof(preferences));
            }

            if (poolRoot == null)
            {
                poolRoot = transform;
            }

            _bound = true;
            Prewarm();
        }

        public bool TryAcquire(out IAudioVoiceLease lease)
        {
            if (!_bound)
            {
                lease = null;
                return false;
            }

            Item item = _available.Count > 0
                ? _available.Pop()
                : null;

            if (item == null &&
                allowGrowth &&
                _all.Count < maxCapacity)
            {
                item = CreateItem();
            }

            if (item == null)
            {
                lease = null;
                return false;
            }

            item.Acquire();
            lease = item;
            return true;
        }

        private void Prewarm()
        {
            int target = Mathf.Min(
                prewarmCount,
                maxCapacity);

            while (_all.Count < target)
            {
                Item item = CreateItem();
                _available.Push(item);
            }
        }

        private Item CreateItem()
        {
            var gameObject = new GameObject(
                $"AudioVoice_{_all.Count:00}");

            gameObject.transform.SetParent(
                poolRoot,
                false);

            AudioSource source =
                gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.dopplerLevel = 0f;

            var item = new Item(
                this,
                source);

            _all.Add(item);
            return item;
        }

        private bool TryGetConfig(
            AudioId id,
            out AudioConfig config)
        {
            config = null;

            if (_catalog == null)
            {
                return false;
            }

            return _catalog.TryGetConfig(
                id,
                out config);
        }

        private void Return(Item item)
        {
            if (item == null ||
                item.IsAvailable)
            {
                return;
            }

            item.ResetForPool();
            _available.Push(item);
        }

        private sealed class Item : IAudioVoiceLease
        {
            private readonly UnityAudioSourcePool _owner;
            private readonly AudioSource _source;

            private bool _released = true;
            private bool _paused;

            public Item(
                UnityAudioSourcePool owner,
                AudioSource source)
            {
                _owner = owner;
                _source = source;
            }

            public bool IsAvailable => _released;

            public bool IsPlaying =>
                !_released &&
                _source != null &&
                _source.isPlaying;

            public bool IsPaused =>
                !_released &&
                _paused;

            public void Acquire()
            {
                _released = false;
                _paused = false;
            }

            public bool TryPlay(
                in AudioDefinition definition,
                in AudioRequest request,
                float volume,
                float pitch)
            {
                if (_released ||
                    !_owner.TryGetConfig(
                        definition.Id,
                        out AudioConfig config))
                {
                    return false;
                }

                AudioClip clip =
                    SelectClip(config.Clips);

                if (clip == null)
                {
                    return false;
                }

                _source.Stop();
                _source.clip = clip;
                _source.outputAudioMixerGroup =
                    config.OutputMixerGroup;

                _source.loop =
                    definition.LifetimeMode ==
                    AudioLifetimeMode.Looping;

                _source.pitch = pitch;
                _source.volume = volume;

                _source.spatialBlend =
                    definition.SpatialMode ==
                    AudioSpatialMode.ThreeD
                        ? 1f
                        : 0f;

                _source.minDistance =
                    definition.MinDistance;

                _source.maxDistance =
                    definition.MaxDistance;

                _source.rolloffMode =
                    AudioRolloffMode.Logarithmic;

                _source.dopplerLevel = 0f;

                if (definition.SpatialMode ==
                    AudioSpatialMode.ThreeD)
                {
                    AudioPoint point =
                        request.Anchor != null
                            ? request.Anchor.Position
                            : request.Position;

                    SetPosition(in point);
                }

                _source.Play();
                return true;
            }

            public void SetPaused(bool paused)
            {
                if (_released ||
                    _paused == paused)
                {
                    return;
                }

                _paused = paused;

                if (paused)
                {
                    _source.Pause();
                }
                else
                {
                    _source.UnPause();
                }
            }

            public void SetVolume(float volume)
            {
                if (!_released)
                {
                    _source.volume = volume;
                }
            }

            public void SetPosition(in AudioPoint position)
            {
                if (_released)
                {
                    return;
                }

                _source.transform.position =
                    new Vector3(
                        position.X,
                        position.Y,
                        position.Z);
            }

            public void Stop()
            {
                if (_released)
                {
                    return;
                }

                _source.Stop();
                _paused = false;
            }

            public void Release()
            {
                if (!_released)
                {
                    _owner.Return(this);
                }
            }

            public void ResetForPool()
            {
                _source.Stop();
                _source.clip = null;
                _source.outputAudioMixerGroup = null;
                _source.loop = false;
                _source.pitch = 1f;
                _source.volume = 1f;
                _source.spatialBlend = 0f;

                _paused = false;
                _released = true;
            }

            private static AudioClip SelectClip(
                AudioClip[] clips)
            {
                if (clips == null ||
                    clips.Length == 0)
                {
                    return null;
                }

                int start = UnityEngine.Random.Range(
                    0,
                    clips.Length);

                for (int offset = 0;
                     offset < clips.Length;
                     offset++)
                {
                    int index =
                        (start + offset) %
                        clips.Length;

                    if (clips[index] != null)
                    {
                        return clips[index];
                    }
                }

                return null;
            }
        }
    }
}
