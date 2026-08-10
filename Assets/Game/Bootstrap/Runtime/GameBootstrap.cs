using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.GameFlow.Commands;
using ZombieWar.GameFlow.Controller;

namespace ZombieWar.Bootstrap
{
    public sealed class GameBootstrap : IStartable
    {
        private readonly GameFlowController _flow;
        private readonly ICommandRegistry _commandRegistry;
        private readonly ChangeGameFlowStateCommandHandler _flowCommandHandler;

        public GameBootstrap(GameFlowController flow, ICommandRegistry commandRegistry, ChangeGameFlowStateCommandHandler flowCommandHandler)
        {
            _flow = flow;
            _commandRegistry = commandRegistry;
            _flowCommandHandler = flowCommandHandler;
        }

        public void Start()
        {
            _commandRegistry.Register<ChangeGameFlowStateCommand>(_flowCommandHandler);
            _flow.Initialize();
        }
    }
}
