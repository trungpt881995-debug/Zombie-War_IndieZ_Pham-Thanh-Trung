using System;
namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnPoint : IEquatable<SpawnPoint>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public SpawnPoint(float x, float y, float z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(x), "SpawnPoint values must be finite.");
            X = x; Y = y; Z = z;
        }
        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        public bool Equals(SpawnPoint other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object obj) => obj is SpawnPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X,Y,Z);
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
