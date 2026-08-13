using System;

namespace ZombieWar.Features.Audio.Domain
{
    public readonly struct AudioDefinition
    {
        public AudioId Id { get; }
        public AudioCategory Category { get; }
        public AudioLifetimeMode LifetimeMode { get; }
        public AudioSpatialMode SpatialMode { get; }
        public AudioPriority Priority { get; }
        public int MaxConcurrent { get; }
        public float BaseVolume { get; }
        public float MinPitch { get; }
        public float MaxPitch { get; }
        public float MinDistance { get; }
        public float MaxDistance { get; }
        public bool AllowDuringTerminalDrain { get; }

        public AudioDefinition(
            AudioId id,
            AudioCategory category,
            AudioLifetimeMode lifetimeMode,
            AudioSpatialMode spatialMode,
            AudioPriority priority,
            int maxConcurrent,
            float baseVolume,
            float minPitch,
            float maxPitch,
            float minDistance,
            float maxDistance,
            bool allowDuringTerminalDrain)
        {
            if (id == AudioId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (maxConcurrent <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrent));
            }

            if (baseVolume < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseVolume));
            }

            if (minPitch <= 0f || maxPitch < minPitch)
            {
                throw new ArgumentOutOfRangeException(nameof(minPitch));
            }

            if (minDistance < 0f || maxDistance < minDistance)
            {
                throw new ArgumentOutOfRangeException(nameof(minDistance));
            }

            Id = id;
            Category = category;
            LifetimeMode = lifetimeMode;
            SpatialMode = spatialMode;
            Priority = priority;
            MaxConcurrent = maxConcurrent;
            BaseVolume = baseVolume;
            MinPitch = minPitch;
            MaxPitch = maxPitch;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            AllowDuringTerminalDrain = allowDuringTerminalDrain;
        }
    }
}
