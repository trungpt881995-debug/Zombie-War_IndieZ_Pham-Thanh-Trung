using System;

namespace ZombieWar.Features.Level.Domain
{
    [Flags] public enum LevelBossObjectiveId
    {
        None = 0, BossA = 1<< 0, BossB = 1<< 1, BossC = 1<< 2
    }
}
