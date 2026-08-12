using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Formation
{
    public readonly struct FormationSlot
    {
        public SoldierPoint LocalPosition { get; }

        public FormationSlot(in SoldierPoint localPosition)
        {
            LocalPosition = localPosition;
        }
    }
}
