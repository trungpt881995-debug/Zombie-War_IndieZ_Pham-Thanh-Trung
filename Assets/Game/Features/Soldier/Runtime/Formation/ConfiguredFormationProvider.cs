using System;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Formation
{
    /// <summary>
    /// Config-backed Strategy. Slot spacing is authored data, never hard-coded
    /// inside SoldierGroupController.
    /// </summary>
    public sealed class ConfiguredFormationProvider : IFormationProvider
    {
        private readonly FormationLayout[] _layouts = new FormationLayout[4];

        public ConfiguredFormationProvider(FormationLayout level1, FormationLayout level2, FormationLayout level3, FormationLayout level4)
        {
            Assign(level1, SoldierGroupLevel.Level1);
            Assign(level2, SoldierGroupLevel.Level2);
            Assign(level3, SoldierGroupLevel.Level3);
            Assign(level4, SoldierGroupLevel.Level4);
        }

        public FormationLayout Get(SoldierGroupLevel level)
        {
            int index = (int)level - 1;

            if (index < 0 || index >= _layouts.Length)
                throw new ArgumentOutOfRangeException(nameof(level));

            return _layouts[index];
        }

        private void Assign(FormationLayout layout, SoldierGroupLevel expectedLevel)
        {
            if (layout == null)
                throw new ArgumentNullException(nameof(layout));

            if (layout.Level != expectedLevel)
            {
                throw new ArgumentException($"Expected {expectedLevel} layout but received {layout.Level}.");
            }

            _layouts[(int)expectedLevel - 1] = layout;
        }
    }
}
