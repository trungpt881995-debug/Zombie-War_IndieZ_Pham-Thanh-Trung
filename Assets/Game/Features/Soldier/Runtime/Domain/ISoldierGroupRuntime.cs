using GameplayCore.Entities;

namespace ZombieWar.Features.Soldier.Domain
{
    public interface ISoldierGroupRuntime
    {
        EntityId GroupId { get; }
        SoldierGroupLevel Level { get; }
        int ActiveSoldierCount { get; }
        bool GameplayEnabled { get; }

        void Tick(float deltaTime);

        bool TryAdvanceTo(SoldierGroupLevel nextLevel);

        void ResetForGameLevel();

        void SetGameplayEnabled(bool enabled);

        SoldierGroupSnapshot Snapshot();
    }
}
