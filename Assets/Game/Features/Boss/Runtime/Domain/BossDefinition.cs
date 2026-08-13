using System;
namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossDefinition
    {
        public BossId Id{get;} public float MaxHealth{get;} public float MoveSpeed{get;} public float RotationSpeed{get;}
        public float AttackDamage{get;} public float AttackRange{get;} public float AttackExitRangeBonus{get;} public float AttackCooldown{get;}
        public float AiDecisionInterval{get;} public float SpawnDuration{get;} public float HitReactionDuration{get;} public float HitReactionMinInterval{get;}
        public float AttackAnimationTimeout{get;} public float DeathDuration{get;} public float Scale{get;} public int RewardScore{get;}
        public float SpawnOffsetX{get;} public float SpawnOffsetY{get;} public float SpawnOffsetZ{get;} public BossAttackType AttackType{get;}
        public BossDefinition(BossId id,float maxHealth,float moveSpeed,float rotationSpeed,float attackDamage,float attackRange,float attackExitRangeBonus,float attackCooldown,float aiDecisionInterval,float spawnDuration,float hitReactionDuration,float hitReactionMinInterval,float attackAnimationTimeout,float deathDuration,float scale,int rewardScore,float spawnOffsetX,float spawnOffsetY,float spawnOffsetZ,BossAttackType attackType=BossAttackType.BasicMelee)
        {
            if(id==BossId.None)throw new ArgumentOutOfRangeException(nameof(id)); if(maxHealth<=0f)throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if(moveSpeed<0f)throw new ArgumentOutOfRangeException(nameof(moveSpeed)); if(rotationSpeed<0f)throw new ArgumentOutOfRangeException(nameof(rotationSpeed));
            if(attackDamage<0f)throw new ArgumentOutOfRangeException(nameof(attackDamage)); if(attackRange<=0f)throw new ArgumentOutOfRangeException(nameof(attackRange));
            if(attackExitRangeBonus<0f)throw new ArgumentOutOfRangeException(nameof(attackExitRangeBonus)); if(attackCooldown<0f)throw new ArgumentOutOfRangeException(nameof(attackCooldown));
            if(aiDecisionInterval<0f)throw new ArgumentOutOfRangeException(nameof(aiDecisionInterval)); if(spawnDuration<0f)throw new ArgumentOutOfRangeException(nameof(spawnDuration));
            if(hitReactionDuration<0f)throw new ArgumentOutOfRangeException(nameof(hitReactionDuration)); if(hitReactionMinInterval<0f)throw new ArgumentOutOfRangeException(nameof(hitReactionMinInterval));
            if(attackAnimationTimeout<=0f)throw new ArgumentOutOfRangeException(nameof(attackAnimationTimeout)); if(deathDuration<0f)throw new ArgumentOutOfRangeException(nameof(deathDuration));
            if(scale<=0f)throw new ArgumentOutOfRangeException(nameof(scale)); if(rewardScore<0)throw new ArgumentOutOfRangeException(nameof(rewardScore));
            Id=id;MaxHealth=maxHealth;MoveSpeed=moveSpeed;RotationSpeed=rotationSpeed;AttackDamage=attackDamage;AttackRange=attackRange;AttackExitRangeBonus=attackExitRangeBonus;AttackCooldown=attackCooldown;
            AiDecisionInterval=aiDecisionInterval;SpawnDuration=spawnDuration;HitReactionDuration=hitReactionDuration;HitReactionMinInterval=hitReactionMinInterval;AttackAnimationTimeout=attackAnimationTimeout;DeathDuration=deathDuration;
            Scale=scale;RewardScore=rewardScore;SpawnOffsetX=spawnOffsetX;SpawnOffsetY=spawnOffsetY;SpawnOffsetZ=spawnOffsetZ;AttackType=attackType;
        }
    }
}
