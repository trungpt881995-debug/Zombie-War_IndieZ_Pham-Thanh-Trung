using System;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponPoint
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public WeaponPoint(float x, float y, float z)
        {
            ValidateFinite(x, nameof(x));
            ValidateFinite(y, nameof(y));
            ValidateFinite(z, nameof(z));
            X = x; Y = y; Z = z;
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
