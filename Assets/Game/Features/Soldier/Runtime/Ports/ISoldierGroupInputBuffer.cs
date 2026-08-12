using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.Ports
{
    /// <summary>
    /// Small state bridge between the Control adapter and Soldier Group.
    /// It is allocation-free and stores only the latest movement intent.
    /// </summary>
    public interface ISoldierGroupInputBuffer
    {
        SoldierMoveInput Current { get; }

        void Set(in SoldierMoveInput input);

        void Clear();
    }
}
