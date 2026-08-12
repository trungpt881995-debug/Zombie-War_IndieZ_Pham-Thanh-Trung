using GeneralCore.Architecture;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    public interface ISoldierGroupView : IView
    {
        SoldierPoint Position { get; }

        void Move(in SoldierMovementStep movement, float deltaTime);
    }
}
