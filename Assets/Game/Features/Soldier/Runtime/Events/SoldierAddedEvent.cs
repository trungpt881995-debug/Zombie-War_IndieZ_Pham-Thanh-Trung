using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Events
{
    public readonly struct SoldierAddedEvent :
        IEvent
    {
        public EntityId GroupId { get; }
        public EntityId SoldierId { get; }
        public int SlotIndex { get; }
        public SoldierGroupLevel GroupLevel { get; }

        public SoldierAddedEvent(EntityId groupId,EntityId soldierId,int slotIndex,SoldierGroupLevel groupLevel)
        {
            GroupId = groupId;
            SoldierId = soldierId;
            SlotIndex = slotIndex;
            GroupLevel = groupLevel;
        }
    }
}
