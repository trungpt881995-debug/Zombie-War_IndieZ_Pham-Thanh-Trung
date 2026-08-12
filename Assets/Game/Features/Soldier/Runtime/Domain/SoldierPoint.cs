using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierPoint
    {
        public static readonly SoldierPoint Zero = new SoldierPoint(0f, 0f, 0f);

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public SoldierPoint(float x, float y, float z)
        {
            if (!IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
            if (!IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
            if (!IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));

            X = x;
            Y = y;
            Z = z;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
