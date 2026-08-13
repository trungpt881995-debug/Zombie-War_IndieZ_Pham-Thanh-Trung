using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Model
{
    public sealed class ScoreModel
    {
        public ScoreState State { get; private set; } = ScoreState.Uninitialized;
        public bool ScoringEnabled { get; private set; }
        public long TotalScore { get; private set; }
        public long LevelScore { get; private set; }
        public long LevelStartTotalScore { get; private set; }
        public ScoreLevelId CurrentLevel { get; private set; } = ScoreLevelId.None;

        public void Initialize()
        {
            State = ScoreState.Ready;
            ScoringEnabled = false;
            TotalScore = 0;
            LevelScore = 0;
            LevelStartTotalScore = 0;
            CurrentLevel = ScoreLevelId.None;
        }

        public void StartRun()
        {
            State = ScoreState.Running;
            ScoringEnabled = true;
            TotalScore = 0;
            LevelScore = 0;
            LevelStartTotalScore = 0;
            CurrentLevel = ScoreLevelId.None;
        }

        public void BeginLevel(ScoreLevelId level)
        {
            CurrentLevel = level;
            LevelStartTotalScore = TotalScore;
            LevelScore = 0;
            ScoringEnabled = true;
        }

        public void ReplayLevel()
        {
            TotalScore = LevelStartTotalScore;
            LevelScore = 0;
            ScoringEnabled = true;
        }

        public void SetScoringEnabled(bool enabled) => ScoringEnabled = enabled;

        public void CommitAward(long newTotal, long newLevelScore)
        {
            TotalScore = newTotal;
            LevelScore = newLevelScore;
        }

        public ScoreSnapshot Snapshot() => new ScoreSnapshot(
            State,
            ScoringEnabled,
            TotalScore,
            LevelScore,
            LevelStartTotalScore,
            CurrentLevel);
    }
}
