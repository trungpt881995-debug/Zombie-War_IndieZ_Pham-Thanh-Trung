using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Events
{
    public readonly struct ScoreChangedEvent : IEvent
    {
        public long PreviousTotal { get; }
        public long CurrentTotal { get; }
        public long Delta { get; }
        public long CurrentLevelScore { get; }
        public ScoreActionId ActionId { get; }
        public EntityId SourceEntityId { get; }

        public ScoreChangedEvent(
            long previousTotal,
            long currentTotal,
            long delta,
            long currentLevelScore,
            ScoreActionId actionId,
            EntityId sourceEntityId)
        {
            PreviousTotal = previousTotal;
            CurrentTotal = currentTotal;
            Delta = delta;
            CurrentLevelScore = currentLevelScore;
            ActionId = actionId;
            SourceEntityId = sourceEntityId;
        }
    }
}
