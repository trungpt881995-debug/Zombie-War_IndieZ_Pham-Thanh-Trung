using System;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponDirection
    {
        private const float Epsilon = 0.000001f;
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public bool HasDirection => X * X + Y * Y + Z * Z > Epsilon;

        public WeaponDirection(float x, float y, float z)
        {
            ValidateFinite(x, nameof(x));
            ValidateFinite(y, nameof(y));
            ValidateFinite(z, nameof(z));
            float sqr = x * x + y * y + z * z;
            if (sqr <= Epsilon)
                throw new ArgumentException("Weapon direction must be non-zero.");
            float inv = 1f / (float)Math.Sqrt(sqr);
            X = x * inv; Y = y * inv; Z = z * inv;
        }

        public static bool TryFromTo(
            in WeaponPoint from,
            in WeaponPoint to,
            out WeaponDirection direction)
        {
            float x = to.X - from.X;
            float y = to.Y - from.Y;
            float z = to.Z - from.Z;
            float sqr = x * x + y * y + z * z;
            if (sqr <= Epsilon)
            {
                direction = default;
                return false;
            }
            direction = new WeaponDirection(x, y, z);
            return true;
        }

        public WeaponDirection RotateYawDegrees(float degrees)
        {
            if (float.IsNaN(degrees) || float.IsInfinity(degrees))
                throw new ArgumentOutOfRangeException(nameof(degrees));
            double radians = degrees * Math.PI / 180.0;
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);
            float x = X * c + Z * s;
            float z = -X * s + Z * c;
            return new WeaponDirection(x, Y, z);
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
