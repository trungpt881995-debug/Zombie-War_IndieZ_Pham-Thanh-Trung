using System;

namespace ZombieWar.Features.Score.Domain
{
    public readonly struct ScoreRuleDefinition
    {
        public ScoreActionId ActionId { get; }
        public long Points { get; }

        public ScoreRuleDefinition(ScoreActionId actionId, long points)
        {
            if (actionId == ScoreActionId.None) throw new ArgumentOutOfRangeException(nameof(actionId));
            if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points));
            ActionId = actionId;
            Points = points;
        }
    }
}
