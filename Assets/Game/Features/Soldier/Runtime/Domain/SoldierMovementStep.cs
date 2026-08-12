using System;

namespace ZombieWar.Features.Soldier.Domain
{
    /// <summary>
    /// Horizontal world-space velocity produced by the movement solver.
    /// SoldierGroupView applies deltaTime and CharacterController gravity.
    /// </summary>
    public readonly struct SoldierMovementStep
    {
        public static readonly SoldierMovementStep Zero = new SoldierMovementStep(0f, 0f, 0f);

        public float VelocityX { get; }
        public float VelocityZ { get; }
        public float NormalizedSpeed { get; }

        public SoldierMovementStep(float velocityX,float velocityZ,float normalizedSpeed)
        {
            if (!IsFinite(velocityX))
                throw new ArgumentOutOfRangeException(nameof(velocityX));

            if (!IsFinite(velocityZ))
                throw new ArgumentOutOfRangeException(nameof(velocityZ));

            if (!IsFinite(normalizedSpeed) || normalizedSpeed < 0f || normalizedSpeed > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(normalizedSpeed));
            }

            VelocityX = velocityX;
            VelocityZ = velocityZ;
            NormalizedSpeed = normalizedSpeed;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
