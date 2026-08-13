using System;
using ZombieWar.Features.Audio.Ports;

namespace ZombieWar.Features.Audio.Services
{
    public sealed class SystemAudioRandom : IAudioRandom
    {
        private readonly Random _random = new Random();

        public float Range(
            float minInclusive,
            float maxInclusive)
        {
            if (maxInclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive +
                   (float)_random.NextDouble() *
                   (maxInclusive - minInclusive);
        }
    }
}
