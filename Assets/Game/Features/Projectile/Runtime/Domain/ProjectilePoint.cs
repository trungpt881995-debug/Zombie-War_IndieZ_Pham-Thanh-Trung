using System;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectilePoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ProjectilePoint(float x, float y, float z)
        {
            ValidateFinite(x, nameof(x));
            ValidateFinite(y, nameof(y));
            ValidateFinite(z, nameof(z));
            X = x; Y = y; Z = z;
        }

        public float DistanceTo(in ProjectilePoint other)
        {
            float dx = other.X - X;
            float dy = other.Y - Y;
            float dz = other.Z - Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
