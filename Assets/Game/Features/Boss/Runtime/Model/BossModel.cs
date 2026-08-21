using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Model
{
    public sealed class BossModel
    {
        public EntityId EntityId
        {
            get;
            private set;
        }
        public BossDefinition Definition
        {
            get;
            private set;
        }
        public BossStateId State
        {
            get;
            private set;
        }
        = BossStateId.Inactive;
        public bool GameplayEnabled
        {
            get;
            private set;
        }
        = true;
        public bool IsTargetable
        {
            get;
            private set;
        }
        public BossTarget CurrentTarget
        {
            get;
            private set;
        }
        = BossTarget.None;
        public EntityId LastDamageSource
        {
            get;
            private set;
        }
        public float HitReactionCooldownRemaining
        {
            get;
            private set;
        }
        public bool ReturnRequested
        {
            get;
            private set;
        }
        public bool IsActive => State != BossStateId.Inactive;
        public void Activate(EntityId id, in BossDefinition definition)
        {
            EntityId = id;
            Definition = definition;
            State = BossStateId.Spawn;
            GameplayEnabled = true;
            IsTargetable = true;
            CurrentTarget = BossTarget.None;
            LastDamageSource = default;
            HitReactionCooldownRemaining = 0f;
            ReturnRequested = false;
        }
        public void Deactivate()
        {
            State = BossStateId.Inactive;
            GameplayEnabled = false;
            IsTargetable = false;
            CurrentTarget = BossTarget.None;
            LastDamageSource = default;
            HitReactionCooldownRemaining = 0f;
            ReturnRequested = false;
        }
        public void SetState(BossStateId s) => State = s;
        public void SetGameplayEnabled(bool e) => GameplayEnabled = e;
        public void SetTargetable(bool v) => IsTargetable = v;
        public void SetTarget(in BossTarget t) => CurrentTarget = t;
        public void ClearTarget() => CurrentTarget = BossTarget.None;
        public void SetLastDamageSource(EntityId id) => LastDamageSource = id;
        public bool CanStartHitReaction => HitReactionCooldownRemaining <= 0f;
        public void BeginHitReactionCooldown() => HitReactionCooldownRemaining = Definition.HitReactionMinInterval;
        public void MarkReturnRequested() => ReturnRequested = true;
        public void TickTimers(float dt)
        {
            if (dt <= 0f || HitReactionCooldownRemaining <= 0f) return;
            HitReactionCooldownRemaining -= dt;
            if (HitReactionCooldownRemaining < 0f) HitReactionCooldownRemaining = 0f;
        }
    }
}
