using GeneralCore.Architecture;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Commands
{
    public readonly struct BeginGameLevelCommand : ICommand
    {
        public GameLevelId GameLevel
        {
            get;
        }
        public BeginGameLevelCommand(GameLevelId gameLevel)
        {
            GameLevel = gameLevel;
        }
    }
}
