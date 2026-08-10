namespace GameplayCore.Random
{
    public interface IGameplayRandom
    {
        uint Seed { get; }
        int Range(int minInclusive, int maxExclusive);
        float Value();
    }

    public sealed class XorShiftGameplayRandom : IGameplayRandom
    {
        private uint _state;
        public uint Seed { get; }

        public XorShiftGameplayRandom() : this(2463534242u) { }

        public XorShiftGameplayRandom(uint seed)
        {
            Seed = seed == 0 ? 2463534242u : seed;
            _state = Seed;
        }

        private uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            var span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % span);
        }

        public float Value() => (NextUInt() & 0x00FFFFFFu) / 16777216f;
    }
}
