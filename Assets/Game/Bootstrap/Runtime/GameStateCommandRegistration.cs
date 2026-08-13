using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.GameState.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class GameStateCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands;
        private readonly BeginGameplayCommandHandler _begin;
        private readonly PauseGameplayCommandHandler _pause;
        private readonly ResumeGameplayCommandHandler _resume;
        private readonly EnterGameOverCommandHandler _gameOver;
        private readonly EnterLevelCompleteCommandHandler _levelComplete;
        private readonly EnterEndGameCommandHandler _endGame;
        private readonly DeactivateGameplayCommandHandler _deactivate;

        public GameStateCommandRegistration(
            ICommandRegistry commands,
            BeginGameplayCommandHandler begin,
            PauseGameplayCommandHandler pause,
            ResumeGameplayCommandHandler resume,
            EnterGameOverCommandHandler gameOver,
            EnterLevelCompleteCommandHandler levelComplete,
            EnterEndGameCommandHandler endGame,
            DeactivateGameplayCommandHandler deactivate)
        {
            _commands = commands;
            _begin = begin;
            _pause = pause;
            _resume = resume;
            _gameOver = gameOver;
            _levelComplete = levelComplete;
            _endGame = endGame;
            _deactivate = deactivate;
        }

        public void Start()
        {
            _commands.Register<BeginGameplayCommand>(_begin);
            _commands.Register<PauseGameplayCommand>(_pause);
            _commands.Register<ResumeGameplayCommand>(_resume);
            _commands.Register<EnterGameOverCommand>(_gameOver);
            _commands.Register<EnterLevelCompleteCommand>(_levelComplete);
            _commands.Register<EnterEndGameCommand>(_endGame);
            _commands.Register<DeactivateGameplayCommand>(_deactivate);
        }
    }
}
