using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    public interface ISoldierTargetingPort
    {
        SoldierTargetInfo Evaluate(EntityId soldierId, in SoldierPoint position, float targetRange);

        void Clear(EntityId soldierId);
    }
}
