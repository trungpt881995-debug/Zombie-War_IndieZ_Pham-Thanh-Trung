using GeneralCore.Architecture;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Events
{
    public readonly struct SoldierGroupLevelChangedEvent : IEvent
    {
        public GameLevelId GameLevel
        {
            get;
        }
        public SoldierGroupLevelId Previous
        {
            get;
        }
        public SoldierGroupLevelId Current
        {
            get;
        }
        public int NormalZombieKillCount
        {
            get;
        }
        public SoldierGroupLevelChangedEvent(GameLevelId gl, SoldierGroupLevelId previous, SoldierGroupLevelId current, int kills)
        {
            GameLevel = gl;
            Previous = previous;
            Current = current;
            NormalZombieKillCount = kills;
        }
    }
}
