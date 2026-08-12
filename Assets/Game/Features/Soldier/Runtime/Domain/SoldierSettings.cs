using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierSettings
    {
        public float MoveSpeed { get; }
        public float AimRotationDegreesPerSecond { get; }

        public SoldierSettings(float moveSpeed, float aimRotationDegreesPerSecond)
        {
            if (!IsFinite(moveSpeed) || moveSpeed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));

            if (!IsFinite(aimRotationDegreesPerSecond) || aimRotationDegreesPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(aimRotationDegreesPerSecond));
            }

            MoveSpeed = moveSpeed;
            AimRotationDegreesPerSecond = aimRotationDegreesPerSecond;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
