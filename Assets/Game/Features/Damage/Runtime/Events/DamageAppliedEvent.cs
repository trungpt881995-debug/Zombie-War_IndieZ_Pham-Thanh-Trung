using GeneralCore.Architecture;
using GameplayCore.Entities;

namespace ZombieWar.Features.Damage.Events
{
    /// <summary>
    /// Cross-feature notification that a resolved damage request was dispatched
    /// to an IDamageable target. Amount is the resolved damage amount, not the
    /// target's actual HP delta after clamping. HealthChangedEvent is the source
    /// of truth when consumers need the actual HP change.
    /// </summary>
    public readonly struct DamageAppliedEvent : IEvent
    {
        public EntityId SourceId { get; }
        public EntityId TargetId { get; }
        public float Amount { get; }
        public string Type { get; }

        public DamageAppliedEvent(EntityId sourceId, EntityId targetId, float amount, string type)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Amount = amount;
            Type = type ?? "Default";
        }
    }
}
