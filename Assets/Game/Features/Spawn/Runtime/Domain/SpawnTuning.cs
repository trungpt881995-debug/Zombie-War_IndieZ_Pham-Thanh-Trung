using System;
namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnTuning : IEquatable<SpawnTuning>
    {
        public int MaxAlive { get; }
        public float Interval { get; }
        public int BatchMin { get; }
        public int BatchMax { get; }
        public SpawnTuning(int maxAlive,float interval,int batchMin,int batchMax)
        {
            if (maxAlive <= 0) 
            throw new ArgumentOutOfRangeException(nameof(maxAlive));

            if (float.IsNaN(interval) || float.IsInfinity(interval) || interval <= 0f) 
            throw new ArgumentOutOfRangeException(nameof(interval));

            if (batchMin <= 0) 
            throw new ArgumentOutOfRangeException(nameof(batchMin));

            if (batchMax < batchMin) 
            throw new ArgumentOutOfRangeException(nameof(batchMax));

            if (batchMax > maxAlive) 
            throw new ArgumentOutOfRangeException(nameof(batchMax));

            MaxAlive=maxAlive; 
            Interval=interval; 
            BatchMin=batchMin; 
            BatchMax=batchMax;
        }
        public bool Equals(SpawnTuning other) => MaxAlive==other.MaxAlive&&Interval.Equals(other.Interval)&&BatchMin==other.BatchMin&&BatchMax==other.BatchMax;
        public override bool Equals(object obj) => obj is SpawnTuning other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MaxAlive,Interval,BatchMin,BatchMax);
    }
}
