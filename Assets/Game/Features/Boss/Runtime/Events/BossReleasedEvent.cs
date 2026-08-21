using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Events
{
    public readonly struct BossReleasedEvent : IEvent
    {
        public BossId BossId
        {
            get;
        }
        public EntityId EntityId
        {
            get;
        }
        public BossReleaseReason Reason
        {
            get;
        }
        public BossReleasedEvent(BossId bossId, EntityId entityId, BossReleaseReason reason)
        {
            BossId = bossId;
            EntityId = entityId;
            Reason = reason;
        }
    }
}
