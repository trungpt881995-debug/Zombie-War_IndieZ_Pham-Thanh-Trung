using System;
using ZombieWar.Features.Audio.Ports;

namespace ZombieWar.Features.Audio.Domain
{
    public enum AudioCategory
    {
        SFX = 0,
        UI = 1,
        Ambience = 2,
        Music = 3
    }

    public enum AudioLifetimeMode
    {
        OneShot = 0,
        Looping = 1
    }

    public enum AudioSpatialMode
    {
        TwoD = 0,
        ThreeD = 1
    }

    public enum WorldAudioMode
    {
        Inactive = 0,
        Playing = 1,
        Suspended = 2,
        TerminalDrain = 3
    }

    public enum AudioPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum AudioFailure
    {
        None = 0,
        NotInitialized = 1,
        InvalidId = 2,
        MissingDefinition = 3,
        InvalidCategory = 4,
        WorldModeRejected = 5,
        ConcurrencyLimited = 6,
        PoolExhausted = 7,
        InvalidSpatialContext = 8,
        PlaybackFailed = 9
    }

    public enum AudioReleaseReason
    {
        Completed = 0,
        Stopped = 1,
        Cancelled = 2,
        ModeChanged = 3,
        AnchorLost = 4,
        VoiceStolen = 5,
        Shutdown = 6
    }

    public readonly struct AudioPoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public AudioPoint(
            float x,
            float y,
            float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public readonly struct AudioHandle : IEquatable<AudioHandle>
    {
        public static readonly AudioHandle Invalid = new AudioHandle(0);

        public long Value { get; }
        public bool IsValid => Value > 0;

        public AudioHandle(long value)
        {
            Value = value;
        }

        public bool Equals(AudioHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(
            AudioHandle left,
            AudioHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            AudioHandle left,
            AudioHandle right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct AudioRequest
    {
        public AudioId Id { get; }
        public AudioPoint Position { get; }
        public bool HasPosition { get; }
        public IAudioAnchor Anchor { get; }
        public float Intensity { get; }
        public long SourceId { get; }

        public AudioRequest(
            AudioId id,
            float intensity = 1f,
            long sourceId = 0)
        {
            Id = id;
            Position = default;
            HasPosition = false;
            Anchor = null;
            Intensity = intensity;
            SourceId = sourceId;
        }

        public AudioRequest(
            AudioId id,
            in AudioPoint position,
            float intensity = 1f,
            long sourceId = 0)
        {
            Id = id;
            Position = position;
            HasPosition = true;
            Anchor = null;
            Intensity = intensity;
            SourceId = sourceId;
        }

        public AudioRequest(
            AudioId id,
            IAudioAnchor anchor,
            float intensity = 1f,
            long sourceId = 0)
        {
            Id = id;
            Position = default;
            HasPosition = false;
            Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            Intensity = intensity;
            SourceId = sourceId;
        }
    }

    public readonly struct AudioPlayResult
    {
        public bool Accepted { get; }
        public AudioHandle Handle { get; }
        public AudioFailure Failure { get; }

        private AudioPlayResult(
            bool accepted,
            AudioHandle handle,
            AudioFailure failure)
        {
            Accepted = accepted;
            Handle = handle;
            Failure = failure;
        }

        public static AudioPlayResult Success(AudioHandle handle)
        {
            return new AudioPlayResult(
                true,
                handle,
                AudioFailure.None);
        }

        public static AudioPlayResult Rejected(AudioFailure failure)
        {
            return new AudioPlayResult(
                false,
                AudioHandle.Invalid,
                failure);
        }
    }
}
