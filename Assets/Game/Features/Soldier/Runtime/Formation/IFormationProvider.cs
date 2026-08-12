using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Formation
{
    public interface IFormationProvider
    {
        FormationLayout Get(SoldierGroupLevel level);
    }
}
