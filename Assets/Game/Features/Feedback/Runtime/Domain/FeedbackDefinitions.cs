using System;

namespace ZombieWar.Features.Feedback.Domain
{
    public readonly struct CameraFeedbackDefinition
    {
        public bool Enabled { get; }
        public FeedbackCameraCue Cue { get; }
        public float OccupancyDuration { get; }

        public CameraFeedbackDefinition(
            bool enabled,
            FeedbackCameraCue cue,
            float occupancyDuration)
        {
            if (enabled && cue == FeedbackCameraCue.None)
            {
                throw new ArgumentOutOfRangeException(nameof(cue));
            }

            ValidateNonNegativeFinite(occupancyDuration, nameof(occupancyDuration));

            Enabled = enabled;
            Cue = cue;
            OccupancyDuration = occupancyDuration;
        }

        private static void ValidateNonNegativeFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    public readonly struct HapticFeedbackDefinition
    {
        public bool Enabled { get; }
        public HapticFeedbackStrength Strength { get; }
        public float Cooldown { get; }

        public HapticFeedbackDefinition(
            bool enabled,
            HapticFeedbackStrength strength,
            float cooldown)
        {
            if (enabled && strength == HapticFeedbackStrength.None)
            {
                throw new ArgumentOutOfRangeException(nameof(strength));
            }

            if (float.IsNaN(cooldown) || float.IsInfinity(cooldown) || cooldown < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldown));
            }

            Enabled = enabled;
            Strength = strength;
            Cooldown = cooldown;
        }
    }

    public readonly struct ScreenFeedbackDefinition
    {
        public bool Enabled { get; }
        public ScreenFeedbackKind Kind { get; }
        public float Intensity { get; }
        public float Duration { get; }

        public ScreenFeedbackDefinition(
            bool enabled,
            ScreenFeedbackKind kind,
            float intensity,
            float duration)
        {
            if (enabled && kind == ScreenFeedbackKind.None)
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (float.IsNaN(intensity) ||
                float.IsInfinity(intensity) ||
                intensity < 0f ||
                intensity > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity));
            }

            if (float.IsNaN(duration) ||
                float.IsInfinity(duration) ||
                duration < 0f ||
                (enabled && duration <= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            Enabled = enabled;
            Kind = kind;
            Intensity = intensity;
            Duration = duration;
        }
    }

    public readonly struct RecoilFeedbackDefinition
    {
        public bool Enabled { get; }
        public float Strength { get; }
        public float Duration { get; }

        public RecoilFeedbackDefinition(
            bool enabled,
            float strength,
            float duration)
        {
            if (float.IsNaN(strength) ||
                float.IsInfinity(strength) ||
                strength < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(strength));
            }

            if (float.IsNaN(duration) ||
                float.IsInfinity(duration) ||
                duration < 0f ||
                (enabled && duration <= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            Enabled = enabled;
            Strength = strength;
            Duration = duration;
        }
    }
}
