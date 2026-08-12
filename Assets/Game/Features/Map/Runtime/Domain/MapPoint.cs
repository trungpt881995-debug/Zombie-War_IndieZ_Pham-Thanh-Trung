using System;

namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapPoint : IEquatable<MapPoint>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public MapPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(MapPoint other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object obj) => obj is MapPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
