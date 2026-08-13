using System;
namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossSpawnRequest
    {
        public BossId BossId{get;} public BossPoint Position{get;} public float Yaw{get;}
        public BossSpawnRequest(BossId bossId,in BossPoint position,float yaw=0f){if(bossId==BossId.None)throw new ArgumentOutOfRangeException(nameof(bossId));BossId=bossId;Position=position;Yaw=yaw;}
    }
}
