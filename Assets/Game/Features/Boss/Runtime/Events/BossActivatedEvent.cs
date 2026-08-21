using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Events
{
    public readonly struct BossActivatedEvent : IEvent
    {
        public BossId BossId
        {
            get;
        }
        public EntityId EntityId
        {
            get;
        }
        public BossActivatedEvent(BossId bossId, EntityId id)
        {
            BossId = bossId;
            EntityId = id;
        }
    }
}
