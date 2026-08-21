using System;

namespace ZombieWar.Features.Level.Domain
{
    public readonly struct SoldierProgressionStep
    {
        public SoldierGroupLevelId Level
        {
            get;
        }
        public int RequiredTotalKills
        {
            get;
        }
        public SoldierProgressionStep(SoldierGroupLevelId level, int requiredTotalKills)
        {
            if ((int) level < 1 || (int) level > 4) throw new ArgumentOutOfRangeException(nameof(level));
            if (requiredTotalKills < 0) throw new ArgumentOutOfRangeException(nameof(requiredTotalKills));
            Level = level;
            RequiredTotalKills = requiredTotalKills;
        }
    }
}
