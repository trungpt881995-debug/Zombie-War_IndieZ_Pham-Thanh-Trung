using GameplayCore.Entities;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Services
{
    public interface IScoreRuntime
    {
        bool IsInitialized { get; }
        ScoreState State { get; }
        bool ScoringEnabled { get; }
        long TotalScore { get; }
        long LevelScore { get; }
        ScoreLevelId CurrentLevel { get; }
        ScoreSnapshot Snapshot { get; }

        void StartRun();
        bool BeginLevel(ScoreLevelId level);
        bool ReplayCurrentLevel();
        ScoreAwardResult Award(ScoreActionId actionId, EntityId sourceEntityId);
        void SetScoringEnabled(bool enabled);
    }
}
