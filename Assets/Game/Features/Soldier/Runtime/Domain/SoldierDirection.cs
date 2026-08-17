using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierDirection
    {
        public static readonly SoldierDirection Zero =
            new SoldierDirection(0f, 0f, 0f);

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public bool HasDirection =>
            (X * X + Y * Y + Z * Z) > 0.000001f;

        public SoldierDirection(
            float x,
            float y,
            float z)
        {
            if (!IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
            if (!IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));
            if (!IsFinite(z)) throw new ArgumentOutOfRangeException(nameof(z));

            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Full XYZ direction for aiming. This preserves vertical offset between
        /// Soldier and the Zombie chest/AimPoint.
        /// </summary>
        public static bool TryCreateNormalized(
            in SoldierPoint from,
            in SoldierPoint to,
            out SoldierDirection direction)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float dz = to.Z - from.Z;
            float sqr = dx * dx + dy * dy + dz * dz;

            if (sqr <= 0.000001f)
            {
                direction = Zero;
                return false;
            }

            float invLength = 1f / (float)Math.Sqrt(sqr);
            direction = new SoldierDirection(
                dx * invLength,
                dy * invLength,
                dz * invLength);

            return true;
        }

        /// <summary>
        /// Preserved for callers that intentionally need planar XZ direction.
        /// </summary>
        public static bool TryCreateNormalizedXZ(
            in SoldierPoint from,
            in SoldierPoint to,
            out SoldierDirection direction)
        {
            float dx = to.X - from.X;
            float dz = to.Z - from.Z;
            float sqr = dx * dx + dz * dz;

            if (sqr <= 0.000001f)
            {
                direction = Zero;
                return false;
            }

            float invLength = 1f / (float)Math.Sqrt(sqr);
            direction = new SoldierDirection(
                dx * invLength,
                0f,
                dz * invLength);

            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
