namespace ZombieWar.Features.Score.Domain
{
    public readonly struct ScoreAwardResult
    {
        public bool Accepted { get; }
        public long AwardedPoints { get; }
        public long TotalScore { get; }
        public long LevelScore { get; }

        public ScoreAwardResult(bool accepted, long awardedPoints, long totalScore, long levelScore)
        {
            Accepted = accepted;
            AwardedPoints = awardedPoints;
            TotalScore = totalScore;
            LevelScore = levelScore;
        }

        public static ScoreAwardResult Rejected(long total, long level) =>
            new ScoreAwardResult(false, 0, total, level);
    }
}
