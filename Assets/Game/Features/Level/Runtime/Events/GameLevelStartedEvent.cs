using GeneralCore.Architecture;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Events
{
    public readonly struct GameLevelStartedEvent : IEvent
    {
        public GameLevelId GameLevel
        {
            get;
        }
        public SoldierGroupLevelId SoldierGroupLevel
        {
            get;
        }
        public GameLevelStartedEvent(GameLevelId gameLevel, SoldierGroupLevelId soldierGroupLevel)
        {
            GameLevel = gameLevel;
            SoldierGroupLevel = soldierGroupLevel;
        }
    }
}
