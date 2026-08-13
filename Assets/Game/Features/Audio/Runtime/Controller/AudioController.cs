using System;
using System.Collections.Generic;
using GeneralCore.Architecture;
using ZombieWar.Features.Audio.Catalog;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Events;
using ZombieWar.Features.Audio.Model;
using ZombieWar.Features.Audio.Policies;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;

namespace ZombieWar.Features.Audio.Controller
{
    public sealed class AudioController
    {
        private struct ActiveInstance
        {
            public IAudioVoiceLease Voice;
            public AudioDefinition Definition;
            public AudioRequest Request;
            public AudioHandle Handle;
        }

        private readonly AudioModel _model;
        private readonly IAudioPreferences _preferences;
        private readonly IAudioConcurrencyPolicy _concurrency;
        private readonly IAudioModePolicy _modePolicy;
        private readonly IAudioRandom _random;
        private readonly IEventBus _events;
        private readonly List<ActiveInstance> _active;

        private IAudioCatalog _catalog;
        private IAudioVoicePool _pool;
        private long _nextHandle = 1;

        public AudioController(
            AudioModel model,
            IAudioPreferences preferences,
            IAudioConcurrencyPolicy concurrency,
            IAudioModePolicy modePolicy,
            IAudioRandom random,
            IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
            _concurrency = concurrency ?? throw new ArgumentNullException(nameof(concurrency));
            _modePolicy = modePolicy ?? throw new ArgumentNullException(nameof(modePolicy));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _active = new List<ActiveInstance>(64);
        }

        public bool IsInitialized => _catalog != null && _pool != null;

        public void Initialize(
            IAudioCatalog catalog,
            IAudioVoicePool pool)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public void Shutdown()
        {
            CancelAll(AudioReleaseReason.Shutdown);
            _catalog = null;
            _pool = null;
            _model.ResetRuntimeState();
        }

        public AudioPlayResult Play(in AudioRequest request)
        {
            if (!IsInitialized)
            {
                return Reject(
                    request.Id,
                    AudioFailure.NotInitialized);
            }

            if (request.Id == AudioId.None)
            {
                return Reject(
                    request.Id,
                    AudioFailure.InvalidId);
            }

            if (!_catalog.TryGet(
                    request.Id,
                    out AudioDefinition definition))
            {
                return Reject(
                    request.Id,
                    AudioFailure.MissingDefinition);
            }

            if (definition.Category == AudioCategory.Music)
            {
                return Reject(
                    request.Id,
                    AudioFailure.InvalidCategory);
            }

            if (!_modePolicy.CanPlay(
                    _model.WorldMode,
                    in definition))
            {
                return Reject(
                    request.Id,
                    AudioFailure.WorldModeRejected);
            }

            if (definition.SpatialMode == AudioSpatialMode.ThreeD &&
                !HasValidSpatialContext(in request))
            {
                return Reject(
                    request.Id,
                    AudioFailure.InvalidSpatialContext);
            }

            int concurrent = CountActive(request.Id);

            if (!_concurrency.CanPlay(
                    in definition,
                    concurrent))
            {
                return Reject(
                    request.Id,
                    AudioFailure.ConcurrencyLimited);
            }

            if (!_pool.TryAcquire(out IAudioVoiceLease voice))
            {
                if (!TryStealLowerPriority(definition.Priority) ||
                    !_pool.TryAcquire(out voice))
                {
                    return Reject(
                        request.Id,
                        AudioFailure.PoolExhausted);
                }
            }

            float pitch = _random.Range(
                definition.MinPitch,
                definition.MaxPitch);

            float volume = CalculateVolume(
                in definition,
                request.Intensity);

            if (!voice.TryPlay(
                    in definition,
                    in request,
                    volume,
                    pitch))
            {
                voice.Release();

                return Reject(
                    request.Id,
                    AudioFailure.PlaybackFailed);
            }

            AudioHandle handle =
                definition.LifetimeMode == AudioLifetimeMode.Looping
                    ? new AudioHandle(_nextHandle++)
                    : AudioHandle.Invalid;

            _active.Add(
                new ActiveInstance
                {
                    Voice = voice,
                    Definition = definition,
                    Request = request,
                    Handle = handle
                });

            _model.SetActiveVoiceCount(_active.Count);
            long sequence = _model.RegisterPlayed();

            _events.Publish(
                new AudioPlayedEvent(
                    request.Id,
                    request.SourceId,
                    handle,
                    sequence));

            return AudioPlayResult.Success(handle);
        }

        public bool IsPlaying(AudioHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Handle == handle)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Stop(AudioHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Handle != handle)
                {
                    continue;
                }

                ReleaseAt(
                    i,
                    AudioReleaseReason.Stopped);

                return true;
            }

            return false;
        }

        public void SetWorldMode(WorldAudioMode mode)
        {
            if (_model.WorldMode == mode)
            {
                return;
            }

            _model.SetWorldMode(mode);

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveInstance instance = _active[i];

                if (instance.Definition.Category == AudioCategory.UI)
                {
                    continue;
                }

                switch (mode)
                {
                    case WorldAudioMode.Playing:
                        instance.Voice.SetPaused(false);
                        break;

                    case WorldAudioMode.Suspended:
                        instance.Voice.SetPaused(true);
                        break;

                    case WorldAudioMode.TerminalDrain:
                        if (instance.Definition.LifetimeMode == AudioLifetimeMode.Looping)
                        {
                            ReleaseAt(
                                i,
                                AudioReleaseReason.ModeChanged);
                        }
                        else
                        {
                            instance.Voice.SetPaused(false);
                        }
                        break;

                    default:
                        ReleaseAt(
                            i,
                            AudioReleaseReason.ModeChanged);
                        break;
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsInitialized)
            {
                return;
            }

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveInstance instance = _active[i];
                IAudioAnchor anchor = instance.Request.Anchor;

                if (anchor != null)
                {
                    if (!anchor.IsValid)
                    {
                        ReleaseAt(
                            i,
                            AudioReleaseReason.AnchorLost);
                        continue;
                    }

                    AudioPoint position = anchor.Position;
                    instance.Voice.SetPosition(in position);
                }

                instance.Voice.SetVolume(
                    CalculateVolume(
                        in instance.Definition,
                        instance.Request.Intensity));

                if (instance.Definition.LifetimeMode == AudioLifetimeMode.OneShot &&
                    !instance.Voice.IsPaused &&
                    !instance.Voice.IsPlaying)
                {
                    ReleaseAt(
                        i,
                        AudioReleaseReason.Completed);
                }
            }
        }

        public void CancelAll(AudioReleaseReason reason)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ReleaseAt(
                    i,
                    reason);
            }
        }

        private AudioPlayResult Reject(
            AudioId id,
            AudioFailure failure)
        {
            _model.RegisterRejected();

            _events.Publish(
                new AudioRejectedEvent(
                    id,
                    failure));

            return AudioPlayResult.Rejected(failure);
        }

        private int CountActive(AudioId id)
        {
            int count = 0;

            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Definition.Id == id)
                {
                    count++;
                }
            }

            return count;
        }

        private bool TryStealLowerPriority(AudioPriority incoming)
        {
            int candidateIndex = -1;
            AudioPriority candidatePriority = incoming;

            for (int i = 0; i < _active.Count; i++)
            {
                ActiveInstance instance = _active[i];

                if (instance.Definition.LifetimeMode == AudioLifetimeMode.Looping ||
                    instance.Definition.Priority >= incoming)
                {
                    continue;
                }

                if (candidateIndex < 0 ||
                    instance.Definition.Priority < candidatePriority)
                {
                    candidateIndex = i;
                    candidatePriority = instance.Definition.Priority;
                }
            }

            if (candidateIndex < 0)
            {
                return false;
            }

            ReleaseAt(
                candidateIndex,
                AudioReleaseReason.VoiceStolen);

            return true;
        }

        private void ReleaseAt(
            int index,
            AudioReleaseReason reason)
        {
            ActiveInstance instance = _active[index];

            instance.Voice.Stop();
            instance.Voice.Release();

            int last = _active.Count - 1;

            if (index != last)
            {
                _active[index] = _active[last];
            }

            _active.RemoveAt(last);
            _model.SetActiveVoiceCount(_active.Count);
            _model.RegisterReleased();

            _events.Publish(
                new AudioReleasedEvent(
                    instance.Definition.Id,
                    reason));
        }

        private float CalculateVolume(
            in AudioDefinition definition,
            float intensity)
        {
            float safeIntensity = intensity;

            if (safeIntensity < 0f)
            {
                safeIntensity = 0f;
            }
            else if (safeIntensity > 1.5f)
            {
                safeIntensity = 1.5f;
            }

            return definition.BaseVolume *
                   safeIntensity *
                   _preferences.GetCategoryVolume(definition.Category);
        }

        private static bool HasValidSpatialContext(in AudioRequest request)
        {
            if (request.Anchor != null)
            {
                return request.Anchor.IsValid;
            }

            return request.HasPosition;
        }
    }
}
