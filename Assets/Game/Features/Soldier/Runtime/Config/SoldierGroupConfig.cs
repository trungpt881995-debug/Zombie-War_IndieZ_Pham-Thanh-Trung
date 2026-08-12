using System;
using UnityEngine;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Formation;

namespace ZombieWar.Features.Soldier.Config
{
    [CreateAssetMenu(
        fileName = "SoldierGroupConfig",
        menuName = "Zombie War/Soldier/Soldier Group Config")]
    public sealed class SoldierGroupConfig : ScriptableObject
    {
        [Header("Formation Level 1 - exactly 1 slot")]
        [SerializeField] private Vector3[] level1Slots = {
            Vector3.zero
        };

        [Header("Formation Level 2 - exactly 2 slots")]
        [SerializeField] private Vector3[] level2Slots = {
            new Vector3(-0.6f, 0f, 0f),
            new Vector3(0.6f, 0f, 0f)
        };

        [Header("Formation Level 3 - exactly 3 slots")]
        [SerializeField] private Vector3[] level3Slots = {
            new Vector3(0f, 0f, 0.6f),
            new Vector3(-0.6f, 0f, -0.4f),
            new Vector3(0.6f, 0f, -0.4f)
        };

        [Header("Formation Level 4 - exactly 4 slots")]
        [SerializeField] private Vector3[] level4Slots = {
            new Vector3(-0.6f, 0f, 0.6f),
            new Vector3(0.6f, 0f, 0.6f),
            new Vector3(-0.6f, 0f, -0.6f),
            new Vector3(0.6f, 0f, -0.6f)
        };

        public IFormationProvider CreateFormationProvider()
        {
            return new ConfiguredFormationProvider(CreateLayout( SoldierGroupLevel.Level1,level1Slots), CreateLayout( SoldierGroupLevel.Level2,level2Slots), CreateLayout(SoldierGroupLevel.Level3,level3Slots), CreateLayout(SoldierGroupLevel.Level4, level4Slots));
        }

        private static FormationLayout CreateLayout(SoldierGroupLevel level, Vector3[] positions)
        {
            if (positions == null)
                throw new InvalidOperationException($"{level} formation is missing.");

            int expected = (int)level;

            if (positions.Length != expected)
            {
                throw new InvalidOperationException($"{level} requires exactly {expected} formation slot(s), " + $"but the config contains {positions.Length}.");
            }

            var slots = new FormationSlot[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 p = positions[i];

                var point = new SoldierPoint(p.x, p.y, p.z);

                slots[i] = new FormationSlot(in point);
            }

            return new FormationLayout(level, slots);
        }
    }
}
