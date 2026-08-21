using System;

namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossSpawnSelection
    {
        public int Count
        {
            get;
        }
        public BossId First
        {
            get;
        }
        public BossId Second
        {
            get;
        }
        public BossSpawnSelection(BossId first)
        {
            if (first == BossId.None) throw new ArgumentOutOfRangeException(nameof(first));
            Count = 1;
            First = first;
            Second = BossId.None;
        }
        public BossSpawnSelection(BossId first, BossId second)
        {
            if (first == BossId.None || second == BossId.None || first == second) throw new ArgumentException("Two distinct Boss IDs are required.");
            Count = 2;
            First = first;
            Second = second;
        }
        public BossId Get(int index)
        {
            if (index == 0) return First;
            if (index == 1 && Count > 1) return Second;
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
