using System;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Formation
{
    public sealed class FormationLayout
    {
        private readonly FormationSlot[] _slots;

        public SoldierGroupLevel Level { get; }

        public int Count => _slots.Length;

        public FormationSlot this[int index] => _slots[index];

        public FormationLayout(SoldierGroupLevel level, FormationSlot[] slots)
        {
            if (slots == null)
                throw new ArgumentNullException(nameof(slots));

            int expected = (int)level;

            if (slots.Length != expected)
            {
                throw new ArgumentException($"Formation {level} requires exactly {expected} slot(s).", nameof(slots));
            }

            Level = level;
            _slots = new FormationSlot[slots.Length];
            Array.Copy(slots, _slots, slots.Length);
        }
    }
}
