namespace ZombieWar.Features.Level.Domain
{
    public readonly struct LevelProgressSnapshot
    {
        public GameLevelId GameLevel { get; }
        public SoldierGroupLevelId SoldierGroupLevel { get; }
        public int NormalZombieKillCount { get; }
        public int NextThreshold { get; }
        public LevelState State { get; }
        public LevelPhase Phase { get; }
        public bool ProgressionEnabled { get; }
        public LevelBossObjectiveId RequiredBossObjectives { get; }
        public LevelBossObjectiveId DefeatedBossObjectives { get; }
        public LevelProgressSnapshot(GameLevelId gameLevel, SoldierGroupLevelId soldierGroupLevel, int normalZombieKillCount, int nextThreshold, LevelState state, LevelPhase phase, bool progressionEnabled, LevelBossObjectiveId requiredBossObjectives, LevelBossObjectiveId defeatedBossObjectives)
        { GameLevel=gameLevel; SoldierGroupLevel=soldierGroupLevel; NormalZombieKillCount=normalZombieKillCount; NextThreshold=nextThreshold; State=state; Phase=phase; ProgressionEnabled=progressionEnabled; RequiredBossObjectives=requiredBossObjectives; DefeatedBossObjectives=defeatedBossObjectives; }
    }
}
