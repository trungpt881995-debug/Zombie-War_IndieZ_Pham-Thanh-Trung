using ZombieWar.Features.Level.Domain;
namespace ZombieWar.Features.Level.Model
{
    public sealed class LevelModel
    {
        public LevelState State { get; private set; } = LevelState.Uninitialized;
        public LevelPhase Phase { get; private set; } = LevelPhase.None;
        public GameLevelId GameLevel { get; private set; } = GameLevelId.None;
        public SoldierGroupLevelId SoldierGroupLevel { get; private set; } = SoldierGroupLevelId.Level1;
        public int NormalZombieKillCount { get; private set; }
        public bool ProgressionEnabled { get; private set; }
        public LevelBossObjectiveId RequiredBossObjectives { get; private set; }
        public LevelBossObjectiveId DefeatedBossObjectives { get; private set; }
        public void SetReady(){ State=LevelState.Ready; Phase=LevelPhase.None; ProgressionEnabled=false; }
        public void Begin(LevelDefinition d){ GameLevel=d.Id; SoldierGroupLevel=SoldierGroupLevelId.Level1; NormalZombieKillCount=0; RequiredBossObjectives=d.RequiredBossObjectives; DefeatedBossObjectives=LevelBossObjectiveId.None; State=LevelState.Running; Phase=LevelPhase.NormalCombat; ProgressionEnabled=true; }
        public void AddKills(int count){ NormalZombieKillCount += count; }
        public void SetSoldierGroupLevel(SoldierGroupLevelId level){ SoldierGroupLevel=level; }
        public void StartBossPhase(){ Phase=LevelPhase.BossPhase; }
        public void SetProgressionEnabled(bool enabled){ ProgressionEnabled=enabled && State==LevelState.Running; }
        public void AddDefeatedBoss(LevelBossObjectiveId boss){ DefeatedBossObjectives |= boss; }
        public void Complete(){ State=LevelState.Completed; Phase=LevelPhase.Completed; ProgressionEnabled=false; }
        public bool BossObjectivesComplete => (DefeatedBossObjectives & RequiredBossObjectives) == RequiredBossObjectives;
        public void Reset(){ State=LevelState.Ready; Phase=LevelPhase.None; GameLevel=GameLevelId.None; SoldierGroupLevel=SoldierGroupLevelId.Level1; NormalZombieKillCount=0; ProgressionEnabled=false; RequiredBossObjectives=LevelBossObjectiveId.None; DefeatedBossObjectives=LevelBossObjectiveId.None; }
    }
}
