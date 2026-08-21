using GeneralCore.Architecture;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Events
{
    public readonly struct GameLevelCompletedEvent : IEvent
    {
        public GameLevelId GameLevel
        {
            get;
        }
        public bool IsFinalLevel
        {
            get;
        }
        public GameLevelCompletedEvent(GameLevelId gl, bool final)
        {
            GameLevel = gl;
            IsFinalLevel = final;
        }
    }
}
