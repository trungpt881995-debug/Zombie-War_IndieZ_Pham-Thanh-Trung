using GeneralCore.Architecture;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    public interface ISoldierView : IView
    {
        SoldierPoint Position { get; }

        void SetActive(bool active);

        void SetLocalFormationPosition(in SoldierPoint localPosition);

        void SetMovementSpeed(float normalizedSpeed);

        void SetAimDirection(in SoldierDirection direction,float rotationDegreesPerSecond,float deltaTime);

        void ClearAim();
    }
}
