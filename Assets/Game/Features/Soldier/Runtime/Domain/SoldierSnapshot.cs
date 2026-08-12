using GameplayCore.Entities;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierSnapshot
    {
        public EntityId EntityId { get; }
        public int SlotIndex { get; }
        public bool Active { get; }

        public SoldierSnapshot(EntityId entityId,int slotIndex,bool active)
        {
            EntityId = entityId;
            SlotIndex = slotIndex;
            Active = active;
        }
    }
}
