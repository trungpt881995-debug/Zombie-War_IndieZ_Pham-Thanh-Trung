using System;

namespace ZombieWar.Features.Control.Domain
{
    public readonly struct MovementIntent
    {
        public static readonly MovementIntent Zero = new MovementIntent(0f, 0f, 0f);

        public float X { get; }
        public float Y { get; }
        public float Magnitude { get; }

        public bool HasInput => Magnitude > 0f;

        public MovementIntent(float x, float y, float magnitude)
        {
            if (float.IsNaN(x) || float.IsInfinity(x))
                throw new ArgumentOutOfRangeException(nameof(x));
            if (float.IsNaN(y) || float.IsInfinity(y))
                throw new ArgumentOutOfRangeException(nameof(y));
            if (float.IsNaN(magnitude) || float.IsInfinity(magnitude) || magnitude < 0f || magnitude > 1f)
                throw new ArgumentOutOfRangeException(nameof(magnitude));

            X = x;
            Y = y;
            Magnitude = magnitude;
        }
    }
}
