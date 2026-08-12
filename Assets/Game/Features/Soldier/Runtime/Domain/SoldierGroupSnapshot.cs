using GameplayCore.Entities;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierGroupSnapshot
    {
        public EntityId GroupId { get; }
        public SoldierGroupLevel Level { get; }
        public int ActiveSoldierCount { get; }
        public bool GameplayEnabled { get; }
        public SoldierMoveInput MoveInput { get; }

        public SoldierGroupSnapshot(EntityId groupId, SoldierGroupLevel level, int activeSoldierCount, bool gameplayEnabled, in SoldierMoveInput moveInput)
        {
            GroupId = groupId;
            Level = level;
            ActiveSoldierCount = activeSoldierCount;
            GameplayEnabled = gameplayEnabled;
            MoveInput = moveInput;
        }
    }
}
