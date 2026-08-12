using System;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Movement
{
    public sealed class SoldierMovementSolver : ISoldierMovementSolver
    {
        public SoldierMovementStep Solve(in SoldierMoveInput input, float moveSpeed)
        {
            if (float.IsNaN(moveSpeed) || float.IsInfinity(moveSpeed) || moveSpeed <= 0f || !input.HasInput)
            {
                return SoldierMovementStep.Zero;
            }

            // Preserve the normalized joystick vector from Control.
            float velocityX = input.X * moveSpeed;

            float velocityZ = input.Y * moveSpeed;

            return new SoldierMovementStep(velocityX, velocityZ, input.Magnitude);
        }
    }
}
