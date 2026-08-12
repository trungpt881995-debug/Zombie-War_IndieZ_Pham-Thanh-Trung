using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Domain
{
    public readonly struct ZombieAttackRequest
    {
        public EntityId AttackerId { get; }
        public EntityId TargetId { get; }
        public float Damage { get; }

        public ZombieAttackRequest(EntityId attackerId, EntityId targetId, float damage)
        {
            AttackerId = attackerId; TargetId = targetId; Damage = damage;
        }
    }
}
