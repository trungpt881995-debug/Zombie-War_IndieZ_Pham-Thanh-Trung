using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierSettings
    {
        public float MoveSpeed { get; }
        public float MoveRotationDegreesPerSecond { get; }
        public float AimRotationDegreesPerSecond { get; }

        // Backward-compatible constructor for existing tests/tools.
        // Movement turning inherits the previous aim rotation speed.
        public SoldierSettings(
            float moveSpeed,
            float aimRotationDegreesPerSecond)
            : this(
                moveSpeed,
                aimRotationDegreesPerSecond,
                aimRotationDegreesPerSecond)
        {
        }

        public SoldierSettings(
            float moveSpeed,
            float moveRotationDegreesPerSecond,
            float aimRotationDegreesPerSecond)
        {
            if (!IsFinite(moveSpeed) || moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));

            if (!IsFinite(moveRotationDegreesPerSecond) ||
                moveRotationDegreesPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveRotationDegreesPerSecond));
            }

            if (!IsFinite(aimRotationDegreesPerSecond) ||
                aimRotationDegreesPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(aimRotationDegreesPerSecond));
            }

            MoveSpeed = moveSpeed;
            MoveRotationDegreesPerSecond =
                moveRotationDegreesPerSecond;
            AimRotationDegreesPerSecond =
                aimRotationDegreesPerSecond;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
