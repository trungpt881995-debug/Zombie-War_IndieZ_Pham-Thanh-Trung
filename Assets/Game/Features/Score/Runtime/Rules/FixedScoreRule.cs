using System;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Rules
{
    public sealed class FixedScoreRule : IScoreRule
    {
        private readonly long _points;
        public ScoreActionId ActionId { get; }

        public FixedScoreRule(ScoreActionId actionId, long points)
        {
            if (actionId == ScoreActionId.None) throw new ArgumentOutOfRangeException(nameof(actionId));
            if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points));
            ActionId = actionId;
            _points = points;
        }

        public long Calculate(in ScoreContext context) => _points;
    }
}
