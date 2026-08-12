using UnityEngine;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Unity.Config
{
    [CreateAssetMenu(fileName = "ZombieConfig", menuName = "Zombie War/Zombie/Zombie Config")]
    public sealed class ZombieConfig : ScriptableObject
    {
        [Header("Stats - Initial Tuning")]
        [SerializeField, Min(0.01f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0f)] private float rotationSpeed = 540f;

        [Header("Combat - Initial Tuning")]
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0.01f)] private float attackRange = 1.4f;
        [SerializeField, Min(0f)] private float attackExitRangeBonus = 0.2f;
        [SerializeField, Min(0f)] private float attackInterval = 1f;
        [SerializeField, Min(0.01f)] private float attackAnimationTimeout = 1.5f;

        [Header("AI - Initial Tuning")]
        [SerializeField, Min(0f)] private float aiDecisionInterval = 0.1f;
        [SerializeField, Min(0f)] private float spawnDuration = 0f;

        [Header("Feedback - Initial Tuning")]
        [SerializeField, Min(0f)] private float hitReactionDuration = 0.15f;
        [SerializeField, Min(0f)] private float hitReactionMinInterval = 0.2f;
        [SerializeField, Min(0f)] private float deathDuration = 1f;
        [SerializeField, Min(0f)] private float dissolveDuration = 0.75f;

        public ZombieDefinition CreateDefinition() => new ZombieDefinition(
            maxHealth, moveSpeed, rotationSpeed, attackDamage, attackRange,
            attackExitRangeBonus, attackInterval, aiDecisionInterval, spawnDuration,
            hitReactionDuration, hitReactionMinInterval, attackAnimationTimeout,
            deathDuration, dissolveDuration);
    }
}
