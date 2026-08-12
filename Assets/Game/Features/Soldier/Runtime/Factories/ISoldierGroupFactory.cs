using System.Collections.Generic;
using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Formation;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Features.Soldier.Factories
{
    public interface ISoldierGroupFactory
    {
        SoldierGroupController Create(ISoldierGroupView groupView, IReadOnlyList<ISoldierView> soldierViews, in SoldierSettings settings, IFormationProvider formationProvider);
    }
}
