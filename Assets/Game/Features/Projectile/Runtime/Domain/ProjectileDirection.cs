using System;

namespace ZombieWar.Features.Projectile.Domain
{
    public readonly struct ProjectileDirection
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ProjectileDirection(float x, float y, float z)
        {
            if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y) || float.IsNaN(z) || float.IsInfinity(z))
                throw new ArgumentOutOfRangeException(nameof(x));

            float sqr = x * x + y * y + z * z;
            if (sqr <= 0.000001f)
                throw new ArgumentException("Projectile direction cannot be zero.");

            float inv = 1f / (float)Math.Sqrt(sqr);
            X = x * inv; Y = y * inv; Z = z * inv;
        }
    }
}
