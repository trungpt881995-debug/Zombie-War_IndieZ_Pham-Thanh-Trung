using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Model
{
    public sealed class ZombieModel
    {
        public EntityId EntityId { get; private set; }
        public ZombieDefinition Definition { get; private set; }
        public ZombieStateId State { get; private set; } = ZombieStateId.Inactive;
        public bool GameplayEnabled { get; private set; } = true;
        public bool IsTargetable { get; private set; }
        public ZombieTarget CurrentTarget { get; private set; } = ZombieTarget.None;
        public EntityId LastDamageSource { get; private set; }
        public float HitReactionCooldownRemaining { get; private set; }
        public bool ReturnRequested { get; private set; }

        public bool IsActive => State != ZombieStateId.Inactive;

        public void Activate(EntityId entityId, in ZombieDefinition definition)
        {
            EntityId = entityId;
            Definition = definition;
            State = ZombieStateId.Spawn;
            GameplayEnabled = true;
            IsTargetable = true;
            CurrentTarget = ZombieTarget.None;
            LastDamageSource = default;
            HitReactionCooldownRemaining = 0f;
            ReturnRequested = false;
        }

        public void Deactivate()
        {
            State = ZombieStateId.Inactive;
            GameplayEnabled = false;
            IsTargetable = false;
            CurrentTarget = ZombieTarget.None;
            HitReactionCooldownRemaining = 0f;
            ReturnRequested = false;
        }

        public void SetState(ZombieStateId state) => State = state;
        public void SetGameplayEnabled(bool enabled) => GameplayEnabled = enabled;
        public void SetTargetable(bool value) => IsTargetable = value;
        public void SetTarget(in ZombieTarget target) => CurrentTarget = target;
        public void ClearTarget() => CurrentTarget = ZombieTarget.None;
        public void SetLastDamageSource(EntityId source) => LastDamageSource = source;
        public void BeginHitReactionCooldown() => HitReactionCooldownRemaining = Definition.HitReactionMinInterval;
        public bool CanStartHitReaction => HitReactionCooldownRemaining <= 0f;
        public void MarkReturnRequested() => ReturnRequested = true;

        public void TickTimers(float deltaTime)
        {
            if (deltaTime <= 0f || HitReactionCooldownRemaining <= 0f) return;
            HitReactionCooldownRemaining -= deltaTime;
            if (HitReactionCooldownRemaining < 0f) HitReactionCooldownRemaining = 0f;
        }
    }
}
