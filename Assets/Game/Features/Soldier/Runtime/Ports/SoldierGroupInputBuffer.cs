using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    public sealed class SoldierGroupInputBuffer : ISoldierGroupInputBuffer
    {
        public SoldierMoveInput Current { get; private set; }

        public SoldierGroupInputBuffer()
        {
            Current = SoldierMoveInput.Zero;
        }

        public void Set(in SoldierMoveInput input)
        {
            Current = input;
        }

        public void Clear()
        {
            Current = SoldierMoveInput.Zero;
        }
    }
}
