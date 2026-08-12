using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Model
{
    public sealed class SoldierModel
    {
        public EntityId EntityId { get; }
        public int SlotIndex { get; private set; }
        public bool Active { get; private set; }

        public SoldierModel(EntityId entityId)
        {
            EntityId = entityId;
            SlotIndex = -1;
            Active = false;
        }

        public void Activate(int slotIndex)
        {
            SlotIndex = slotIndex;
            Active = true;
        }

        public void SetSlot(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public void Deactivate()
        {
            Active = false;
        }

        public SoldierSnapshot Snapshot()
        {
            return new SoldierSnapshot(EntityId, SlotIndex, Active);
        }
    }
}
