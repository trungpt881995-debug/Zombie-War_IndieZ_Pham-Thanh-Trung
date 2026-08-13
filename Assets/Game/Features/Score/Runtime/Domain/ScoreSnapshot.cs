namespace ZombieWar.Features.Score.Domain
{
    public readonly struct ScoreSnapshot
    {
        public ScoreState State { get; }
        public bool ScoringEnabled { get; }
        public long TotalScore { get; }
        public long LevelScore { get; }
        public long LevelStartTotalScore { get; }
        public ScoreLevelId CurrentLevel { get; }

        public ScoreSnapshot(
            ScoreState state,
            bool scoringEnabled,
            long totalScore,
            long levelScore,
            long levelStartTotalScore,
            ScoreLevelId currentLevel)
        {
            State = state;
            ScoringEnabled = scoringEnabled;
            TotalScore = totalScore;
            LevelScore = levelScore;
            LevelStartTotalScore = levelStartTotalScore;
            CurrentLevel = currentLevel;
        }
    }
}
