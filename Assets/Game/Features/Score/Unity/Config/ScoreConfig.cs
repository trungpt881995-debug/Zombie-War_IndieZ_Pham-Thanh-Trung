using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieWar.Features.Score.Catalog;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Unity.Config
{
    [CreateAssetMenu(menuName = "Zombie War/Score/Score Config", fileName = "ScoreConfig")]
    public sealed class ScoreConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ScoreActionId actionId;
            [Min(1)] public long points;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        public IReadOnlyList<Entry> Entries => entries;

        public ScoreRuleCatalog BuildCatalog()
        {
            var definitions = new ScoreRuleDefinition[entries != null ? entries.Length : 0];
            for (int i = 0; i < definitions.Length; i++)
                definitions[i] = new ScoreRuleDefinition(entries[i].actionId, entries[i].points);
            return new ScoreRuleCatalog(definitions);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Development placeholders only. Tune these values before release.
            entries = new[]
            {
                new Entry { actionId = ScoreActionId.NormalZombieDefeated, points = 10 },
                new Entry { actionId = ScoreActionId.BossADefeated, points = 100 },
                new Entry { actionId = ScoreActionId.BossBDefeated, points = 200 },
                new Entry { actionId = ScoreActionId.BossCDefeated, points = 300 }
            };
        }
#endif
    }
}
