using System;

namespace ZombieWar.Features.Level.Domain
{
    public sealed class LevelDefinition
    {
        private readonly SoldierProgressionStep[] _steps;
        public GameLevelId Id
        {
            get;
        }
        public bool IsFinalLevel
        {
            get;
        }
        public int BossPhaseKillThreshold
        {
            get;
        }
        public LevelBossObjectiveId RequiredBossObjectives
        {
            get;
        }
        public int ProgressionStepCount => _steps.Length;
        public LevelDefinition(GameLevelId id, bool isFinalLevel, SoldierProgressionStep[] steps, int bossPhaseKillThreshold,
        LevelBossObjectiveId requiredBossObjectives)
        {
            if (id == GameLevelId.None) throw new ArgumentException("Level id cannot be None.", nameof(id));
            if (steps == null || steps.Length != 4) throw new ArgumentException("Exactly four Soldier progression steps are required.",
            nameof(steps));
            if (bossPhaseKillThreshold <= 0) throw new ArgumentOutOfRangeException(nameof(bossPhaseKillThreshold));
            if (requiredBossObjectives == LevelBossObjectiveId.None) throw new ArgumentException("At least one Boss objective is required.",
            nameof(requiredBossObjectives));
            _steps = new SoldierProgressionStep[steps.Length];
            int previousKills = - 1;
            for (int i = 0; i < steps.Length; i++)
            {
                if ((int) steps[i].Level != i + 1) throw new ArgumentException("Progression must contain Level1..Level4 in order.",
                nameof(steps));
                if (steps[i].RequiredTotalKills <= previousKills) throw new ArgumentException("Progression thresholds must be strictly increasing.",
                nameof(steps));
                _steps[i] = steps[i];
                previousKills = steps[i].RequiredTotalKills;
            }
            if (_steps[0].RequiredTotalKills != 0) throw new ArgumentException("Level1 threshold must be zero.", nameof(steps));
            if (bossPhaseKillThreshold <= _steps[_steps.Length - 1].RequiredTotalKills) throw new ArgumentException("Boss threshold must be greater than Level4 threshold.",
            nameof(bossPhaseKillThreshold));
            Id = id;
            IsFinalLevel = isFinalLevel;
            BossPhaseKillThreshold = bossPhaseKillThreshold;
            RequiredBossObjectives = requiredBossObjectives;
        }
        public SoldierProgressionStep GetProgressionStep(int index)
        {
            if (index < 0 || index >= _steps.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return _steps[index];
        }
        public int GetNextThreshold(SoldierGroupLevelId currentLevel)
        {
            int index = (int) currentLevel;
            return index < _steps.Length ? _steps[index].RequiredTotalKills : BossPhaseKillThreshold;
        }
    }
}
