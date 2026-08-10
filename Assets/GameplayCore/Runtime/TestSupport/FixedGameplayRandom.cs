using GameplayCore.Random;

namespace GameplayCore.TestSupport
{
    public sealed class FixedGameplayRandom : IGameplayRandom
    {
        private readonly float _value;
        public uint Seed { get; }
        public FixedGameplayRandom(float value = 0.5f, uint seed = 1u) { _value = value; Seed = seed; }
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            var t = _value < 0f ? 0f : (_value >= 1f ? 0.999999f : _value);
            return minInclusive + (int)((maxExclusive - minInclusive) * t);
        }
        public float Value() => _value;
    }
}
