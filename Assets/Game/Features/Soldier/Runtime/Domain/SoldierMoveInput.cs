using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierMoveInput
    {
        public static readonly SoldierMoveInput Zero = new SoldierMoveInput(0f, 0f, 0f);

        public float X { get; }
        public float Y { get; }
        public float Magnitude { get; }

        public bool HasInput => Magnitude > 0f;

        public SoldierMoveInput(float x, float y, float magnitude)
        {
            if (!IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x));
            if (!IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y));

            if (!IsFinite(magnitude) || magnitude < 0f || magnitude > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(magnitude));
            }

            X = x;
            Y = y;
            Magnitude = magnitude;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
