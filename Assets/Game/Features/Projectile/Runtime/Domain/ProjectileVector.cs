using System;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileVector
    {
        public static readonly ProjectileVector Zero = new ProjectileVector(0f, 0f, 0f);
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ProjectileVector(float x, float y, float z)
        {
            ValidateFinite(x, nameof(x));
            ValidateFinite(y, nameof(y));
            ValidateFinite(z, nameof(z));
            X = x; Y = y; Z = z;
        }

        public float SqrMagnitude => X * X + Y * Y + Z * Z;
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
