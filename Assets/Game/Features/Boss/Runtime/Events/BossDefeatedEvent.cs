using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Events
{
    public readonly struct BossDefeatedEvent : IEvent
    {
        public BossId BossId
        {
            get;
        }
        public EntityId EntityId
        {
            get;
        }
        public EntityId Killer
        {
            get;
        }
        public int RewardScore
        {
            get;
        }
        public BossDefeatedEvent(BossId bossId, EntityId entityId, EntityId killer, int rewardScore)
        {
            BossId = bossId;
            EntityId = entityId;
            Killer = killer;
            RewardScore = rewardScore;
        }
    }
}
