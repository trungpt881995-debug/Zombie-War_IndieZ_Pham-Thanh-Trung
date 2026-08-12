using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    public sealed class NullSoldierTargetingPort : ISoldierTargetingPort
    {
        public static readonly NullSoldierTargetingPort Instance = new NullSoldierTargetingPort();

        private NullSoldierTargetingPort()
        {
        }

        public SoldierTargetInfo Evaluate(EntityId soldierId, in SoldierPoint position, float targetRange)
        {
            return SoldierTargetInfo.None;
        }

        public void Clear(EntityId soldierId)
        {
        }
    }
}
