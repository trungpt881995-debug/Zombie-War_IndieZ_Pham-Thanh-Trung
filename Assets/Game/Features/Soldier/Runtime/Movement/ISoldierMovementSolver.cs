using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Movement
{
    public interface ISoldierMovementSolver
    {
        SoldierMovementStep Solve(in SoldierMoveInput input, float moveSpeed);
    }
}
