using System;
using System.Collections.Generic;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Rules;

namespace ZombieWar.Features.Score.Catalog
{
    public sealed class ScoreRuleCatalog : IScoreRuleCatalog
    {
        private static readonly ScoreActionId[] RequiredActions =
        {
            ScoreActionId.NormalZombieDefeated,
            ScoreActionId.BossADefeated,
            ScoreActionId.BossBDefeated,
            ScoreActionId.BossCDefeated
        };

        private readonly Dictionary<ScoreActionId, IScoreRule> _rules;
        public int Count => _rules.Count;

        public ScoreRuleCatalog(IReadOnlyList<ScoreRuleDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            _rules = new Dictionary<ScoreActionId, IScoreRule>(definitions.Count);

            for (int i = 0; i < definitions.Count; i++)
            {
                ScoreRuleDefinition definition = definitions[i];
                if (definition.ActionId == ScoreActionId.None)
                    throw new ArgumentException("Score action cannot be None.", nameof(definitions));
                if (definition.Points <= 0)
                    throw new ArgumentException("Score points must be greater than zero.", nameof(definitions));
                if (_rules.ContainsKey(definition.ActionId))
                    throw new ArgumentException("Duplicate score action: " + definition.ActionId, nameof(definitions));

                _rules.Add(definition.ActionId, new FixedScoreRule(definition.ActionId, definition.Points));
            }

            for (int i = 0; i < RequiredActions.Length; i++)
            {
                if (!_rules.ContainsKey(RequiredActions[i]))
                    throw new ArgumentException("Missing required score action: " + RequiredActions[i], nameof(definitions));
            }
        }

        public bool TryGet(ScoreActionId actionId, out IScoreRule rule) =>
            _rules.TryGetValue(actionId, out rule);
    }
}
