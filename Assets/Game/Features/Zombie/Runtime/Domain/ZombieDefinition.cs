using System;

namespace ZombieWar.Features.Zombie.Domain
{
    public readonly struct ZombieDefinition
    {
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float RotationSpeed { get; }
        public float AttackDamage { get; }
        public float AttackRange { get; }
        public float AttackExitRangeBonus { get; }
        public float AttackInterval { get; }
        public float AiDecisionInterval { get; }
        public float SpawnDuration { get; }
        public float HitReactionDuration { get; }
        public float HitReactionMinInterval { get; }
        public float AttackAnimationTimeout { get; }
        public float DeathDuration { get; }
        public float DissolveDuration { get; }

        public ZombieDefinition(
            float maxHealth,
            float moveSpeed,
            float rotationSpeed,
            float attackDamage,
            float attackRange,
            float attackExitRangeBonus,
            float attackInterval,
            float aiDecisionInterval,
            float spawnDuration,
            float hitReactionDuration,
            float hitReactionMinInterval,
            float attackAnimationTimeout,
            float deathDuration,
            float dissolveDuration)
        {
            if (maxHealth <= 0f) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (moveSpeed < 0f) throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            if (rotationSpeed < 0f) throw new ArgumentOutOfRangeException(nameof(rotationSpeed));
            if (attackDamage < 0f) throw new ArgumentOutOfRangeException(nameof(attackDamage));
            if (attackRange <= 0f) throw new ArgumentOutOfRangeException(nameof(attackRange));
            if (attackExitRangeBonus < 0f) throw new ArgumentOutOfRangeException(nameof(attackExitRangeBonus));
            if (attackInterval < 0f) throw new ArgumentOutOfRangeException(nameof(attackInterval));
            if (aiDecisionInterval < 0f) throw new ArgumentOutOfRangeException(nameof(aiDecisionInterval));
            if (spawnDuration < 0f) throw new ArgumentOutOfRangeException(nameof(spawnDuration));
            if (hitReactionDuration < 0f) throw new ArgumentOutOfRangeException(nameof(hitReactionDuration));
            if (hitReactionMinInterval < 0f) throw new ArgumentOutOfRangeException(nameof(hitReactionMinInterval));
            if (attackAnimationTimeout <= 0f) throw new ArgumentOutOfRangeException(nameof(attackAnimationTimeout));
            if (deathDuration < 0f) throw new ArgumentOutOfRangeException(nameof(deathDuration));
            if (dissolveDuration < 0f) throw new ArgumentOutOfRangeException(nameof(dissolveDuration));

            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            RotationSpeed = rotationSpeed;
            AttackDamage = attackDamage;
            AttackRange = attackRange;
            AttackExitRangeBonus = attackExitRangeBonus;
            AttackInterval = attackInterval;
            AiDecisionInterval = aiDecisionInterval;
            SpawnDuration = spawnDuration;
            HitReactionDuration = hitReactionDuration;
            HitReactionMinInterval = hitReactionMinInterval;
            AttackAnimationTimeout = attackAnimationTimeout;
            DeathDuration = deathDuration;
            DissolveDuration = dissolveDuration;
        }
    }
}
