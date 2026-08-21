using System;
using UnityEngine;
using ZombieWar.Features.Level.Domain;
namespace ZombieWar.Features.Level.Unity.Config
{
    [CreateAssetMenu(menuName = "Zombie War/Level/Level Config", fileName = "LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        [SerializeField] private GameLevelId gameLevel = GameLevelId.GameLevel01;
        [SerializeField] private bool isFinalLevel;
        [SerializeField, Min(1)] private int level2Kills = 200;
        [SerializeField, Min(1)] private int level3Kills = 700;
        [SerializeField, Min(1)] private int level4Kills = 1500;
        [SerializeField, Min(1)] private int bossPhaseKills = 2500;
        [SerializeField] private LevelBossObjectiveId requiredBossObjectives = LevelBossObjectiveId.BossA;
        public GameLevelId GameLevel => gameLevel;
        public LevelDefinition BuildDefinition()
        {
            var steps = new[] { new SoldierProgressionStep(SoldierGroupLevelId.Level1, 0), new SoldierProgressionStep(SoldierGroupLevelId.Level2, level2Kills), new SoldierProgressionStep(SoldierGroupLevelId.Level3, level3Kills), new SoldierProgressionStep(SoldierGroupLevelId.Level4, level4Kills) };
            return new LevelDefinition(gameLevel, isFinalLevel, steps, bossPhaseKills, requiredBossObjectives);
        }
    }
}
