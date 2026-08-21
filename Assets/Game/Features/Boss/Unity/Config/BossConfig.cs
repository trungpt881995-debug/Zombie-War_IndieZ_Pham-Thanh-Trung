using UnityEngine;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Unity.Config
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Zombie War/Boss/Boss Config")] public sealed class BossConfig : ScriptableObject
    {
        [SerializeField] private BossId bossId = BossId.BossA;
        [Header("Stats - Initial Tuning")][SerializeField, Min(1f)] private float maxHealth = 2000f;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float rotationSpeed = 360f;
        [SerializeField, Min(0.01f)] private float scale = 2.5f;
        [Header("Combat - Initial Tuning")][SerializeField, Min(0f)] private float attackDamage = 25f;
        [SerializeField, Min(0.01f)] private float attackRange = 2f;
        [SerializeField, Min(0f)] private float attackExitRangeBonus = 0.3f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.5f;
        [SerializeField, Min(0.01f)] private float attackAnimationTimeout = 2f;
        [SerializeField] private BossAttackType attackType = BossAttackType.BasicMelee;
        [Header("AI")][SerializeField, Min(0f)] private float aiDecisionInterval = 0.1f;
        [SerializeField, Min(0f)] private float spawnDuration = 1f;
        [Header("Feedback")][SerializeField, Min(0f)] private float hitReactionDuration = 0.12f;
        [SerializeField, Min(0f)] private float hitReactionMinInterval = 0.35f;
        [SerializeField, Min(0f)] private float deathDuration = 2f;
        [Header("Level integration")][SerializeField] private Vector3 spawnOffset;
        [SerializeField, Min(0)] private int rewardScore = 1000;
        public BossId BossId => bossId;
        public BossDefinition CreateDefinition() => new BossDefinition(bossId, maxHealth, moveSpeed, rotationSpeed, attackDamage,
        attackRange, attackExitRangeBonus, attackCooldown, aiDecisionInterval, spawnDuration, hitReactionDuration, hitReactionMinInterval,
        attackAnimationTimeout, deathDuration, scale, rewardScore, spawnOffset.x, spawnOffset.y, spawnOffset.z, attackType);
    }
}
