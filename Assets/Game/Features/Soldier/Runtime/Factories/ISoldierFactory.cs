using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Factories
{
    public interface ISoldierFactory
    {
        SoldierController Create(int slotIndex, ISoldierView view, in SoldierSettings settings);
    }
}
