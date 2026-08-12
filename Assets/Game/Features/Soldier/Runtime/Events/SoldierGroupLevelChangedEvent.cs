using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Events
{
    public readonly struct SoldierGroupLevelChangedEvent :
        IEvent
    {
        public EntityId GroupId { get; }
        public SoldierGroupLevel PreviousLevel { get; }
        public SoldierGroupLevel CurrentLevel { get; }

        public SoldierGroupLevelChangedEvent(EntityId groupId, SoldierGroupLevel previousLevel, SoldierGroupLevel currentLevel)
        {
            GroupId = groupId;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }
    }
}
