using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    public sealed class NullSoldierAttackPort : ISoldierAttackPort
    {
        public static readonly NullSoldierAttackPort Instance = new NullSoldierAttackPort();

        private NullSoldierAttackPort()
        {
        }

        public void Update(EntityId soldierId, in SoldierTargetInfo target, float deltaTime)
        {
        }

        public void ClearTarget(EntityId soldierId)
        {
        }
    }
}
