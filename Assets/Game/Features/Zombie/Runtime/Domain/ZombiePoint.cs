using System;

namespace ZombieWar.Features.Zombie.Domain
{
    public readonly struct ZombiePoint : IEquatable<ZombiePoint>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ZombiePoint(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public static float SqrDistanceXZ(in ZombiePoint a, in ZombiePoint b)
        {
            float x = b.X - a.X;
            float z = b.Z - a.Z;
            return x * x + z * z;
        }

        public bool Equals(ZombiePoint other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object obj) => obj is ZombiePoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    }
}
