using System;

namespace ZombieWar.Features.Camera.Domain
{
    public readonly struct CameraShakeDefinition : IEquatable<CameraShakeDefinition>
    {
        public CameraShakeId Id { get; }
        public float Amplitude { get; }
        public float Frequency { get; }
        public float Duration { get; }

        public CameraShakeDefinition(CameraShakeId id, float amplitude, float frequency, float duration)
        {
            if (id == CameraShakeId.None) throw new ArgumentOutOfRangeException(nameof(id));
            if (!IsFinite(amplitude) || amplitude < 0f) throw new ArgumentOutOfRangeException(nameof(amplitude));
            if (!IsFinite(frequency) || frequency < 0f) throw new ArgumentOutOfRangeException(nameof(frequency));
            if (!IsFinite(duration) || duration <= 0f) throw new ArgumentOutOfRangeException(nameof(duration));
            Id = id;
            Amplitude = amplitude;
            Frequency = frequency;
            Duration = duration;
        }

        public bool Equals(CameraShakeDefinition other) =>
            Id == other.Id && Amplitude.Equals(other.Amplitude) &&
            Frequency.Equals(other.Frequency) && Duration.Equals(other.Duration);

        public override bool Equals(object obj) => obj is CameraShakeDefinition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Id, Amplitude, Frequency, Duration);

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
