using System; using GameplayCore.Entities;
namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossAttackRequest
    {
        public EntityId AttackerId{get;} public EntityId TargetId{get;} public float Damage{get;} public BossAttackType AttackType{get;}
        public BossAttackRequest(EntityId attackerId,EntityId targetId,float damage,BossAttackType attackType){if(damage<0f)throw new ArgumentOutOfRangeException(nameof(damage));AttackerId=attackerId;TargetId=targetId;Damage=damage;AttackType=attackType;}
    }
}
