using System;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierSettings
    {
        public const float DefaultBodyTurnEnterAimAngleDegrees = 100f;
        public const float DefaultBodyTurnReleaseAimAngleDegrees = 80f;

        public float MoveSpeed { get; }
        public float MoveRotationDegreesPerSecond { get; }
        public float AimRotationDegreesPerSecond { get; }
        public float BodyTurnEnterAimAngleDegrees { get; }
        public float BodyTurnReleaseAimAngleDegrees { get; }

        // Backward-compatible constructor for existing tests/tools.
        // Movement turning inherits the previous aim rotation speed.
        public SoldierSettings(
            float moveSpeed,
            float aimRotationDegreesPerSecond)
            : this(
                moveSpeed,
                aimRotationDegreesPerSecond,
                aimRotationDegreesPerSecond,
                DefaultBodyTurnEnterAimAngleDegrees,
                DefaultBodyTurnReleaseAimAngleDegrees)
        {
        }

        // Backward-compatible constructor for the current production call sites.
        public SoldierSettings(
            float moveSpeed,
            float moveRotationDegreesPerSecond,
            float aimRotationDegreesPerSecond)
            : this(
                moveSpeed,
                moveRotationDegreesPerSecond,
                aimRotationDegreesPerSecond,
                DefaultBodyTurnEnterAimAngleDegrees,
                DefaultBodyTurnReleaseAimAngleDegrees)
        {
        }

        public SoldierSettings(
            float moveSpeed,
            float moveRotationDegreesPerSecond,
            float aimRotationDegreesPerSecond,
            float bodyTurnEnterAimAngleDegrees,
            float bodyTurnReleaseAimAngleDegrees)
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

            ValidateAngle(
                bodyTurnEnterAimAngleDegrees,
                nameof(bodyTurnEnterAimAngleDegrees));

            ValidateAngle(
                bodyTurnReleaseAimAngleDegrees,
                nameof(bodyTurnReleaseAimAngleDegrees));

            if (bodyTurnReleaseAimAngleDegrees > bodyTurnEnterAimAngleDegrees)
            {
                throw new ArgumentException(
                    "Body-turn release angle must be less than or equal to the enter angle.",
                    nameof(bodyTurnReleaseAimAngleDegrees));
            }

            MoveSpeed = moveSpeed;
            MoveRotationDegreesPerSecond = moveRotationDegreesPerSecond;
            AimRotationDegreesPerSecond = aimRotationDegreesPerSecond;
            BodyTurnEnterAimAngleDegrees = bodyTurnEnterAimAngleDegrees;
            BodyTurnReleaseAimAngleDegrees = bodyTurnReleaseAimAngleDegrees;
        }

        private static void ValidateAngle(float value, string name)
        {
            if (!IsFinite(value) || value < 0f || value > 180f)
                throw new ArgumentOutOfRangeException(name);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
