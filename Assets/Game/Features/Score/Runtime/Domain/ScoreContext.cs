using GameplayCore.Entities;

namespace ZombieWar.Features.Score.Domain
{
    public readonly struct ScoreContext
    {
        public ScoreActionId ActionId { get; }
        public EntityId SourceEntityId { get; }
        public ScoreLevelId Level { get; }

        public ScoreContext(ScoreActionId actionId, EntityId sourceEntityId, ScoreLevelId level)
        {
            ActionId = actionId;
            SourceEntityId = sourceEntityId;
            Level = level;
        }
    }
}
