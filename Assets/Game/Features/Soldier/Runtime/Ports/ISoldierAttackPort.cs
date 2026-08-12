using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    public interface ISoldierAttackPort
    {
        void Update(EntityId soldierId, in SoldierTargetInfo target, float deltaTime);

        void ClearTarget(EntityId soldierId);
    }
}
