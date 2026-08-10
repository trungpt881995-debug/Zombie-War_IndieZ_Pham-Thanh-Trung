using GeneralCore.Architecture;
using ZombieWar.GameFlow.Controller;
using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.Commands
{
    public sealed class ChangeGameFlowStateCommandHandler : ICommandHandler<ChangeGameFlowStateCommand>
    {
        private readonly GameFlowController _controller;
        public ChangeGameFlowStateCommandHandler(GameFlowController controller) => _controller = controller;

        public void Handle(ChangeGameFlowStateCommand command)
        {
            switch (command.Target)
            {
                case GameFlowStateId.MainMenu: _controller.GoToMainMenu(); break;
                case GameFlowStateId.Loading: _controller.BeginLoading(); break;
                case GameFlowStateId.Gameplay: _controller.BeginGameplay(); break;
                case GameFlowStateId.Paused: _controller.Pause(); break;
                case GameFlowStateId.LevelComplete: _controller.LevelComplete(); break;
                case GameFlowStateId.GameOver: _controller.GameOver(); break;
                case GameFlowStateId.EndGame: _controller.EndGame(); break;
            }
        }
    }
}
