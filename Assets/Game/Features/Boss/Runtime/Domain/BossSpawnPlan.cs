using System;
namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossSpawnPlan
    {
        public int Count{get;} public BossSpawnRequest First{get;} public BossSpawnRequest Second{get;}
        public BossSpawnPlan(in BossSpawnRequest first){Count=1;First=first;Second=default;}
        public BossSpawnPlan(in BossSpawnRequest first,in BossSpawnRequest second){if(first.BossId==second.BossId)throw new ArgumentException("Boss spawn plan cannot contain duplicate Boss IDs.");Count=2;First=first;Second=second;}
        public BossSpawnRequest Get(int index){if(index==0)return First;if(index==1&&Count>1)return Second;throw new ArgumentOutOfRangeException(nameof(index));}
    }
}
