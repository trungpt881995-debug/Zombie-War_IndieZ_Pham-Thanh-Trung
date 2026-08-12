using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Model
{
    public sealed class SoldierGroupModel
    {
        public EntityId GroupId { get; }

        public SoldierGroupLevel Level { get; private set; }

        public SoldierMoveInput MoveInput { get; private set; }

        public bool GameplayEnabled { get; private set; }

        public int RequiredSoldierCount => (int)Level;

        public SoldierGroupModel(EntityId groupId)
        {
            GroupId = groupId;
            Reset();
        }

        public void SetMoveInput(in SoldierMoveInput input)
        {
            MoveInput = GameplayEnabled ? input : SoldierMoveInput.Zero;
        }

        public bool TryAdvanceTo(SoldierGroupLevel nextLevel)
        {
            int expected = (int)Level + 1;

            if ((int)nextLevel != expected || (int)nextLevel > (int)SoldierGroupLevel.Level4)
            {
                return false;
            }

            Level = nextLevel;
            return true;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            GameplayEnabled = enabled;

            if (!enabled)
                MoveInput = SoldierMoveInput.Zero;
        }

        public void Reset()
        {
            Level = SoldierGroupLevel.Level1;
            MoveInput = SoldierMoveInput.Zero;
            GameplayEnabled = true;
        }

        public SoldierGroupSnapshot Snapshot()
        {
            return new SoldierGroupSnapshot(GroupId, Level, RequiredSoldierCount, GameplayEnabled, MoveInput);
        }
    }
}
