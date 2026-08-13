using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class EnterLevelCompleteCommandHandler : ICommandHandler<EnterLevelCompleteCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public EnterLevelCompleteCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(EnterLevelCompleteCommand command) => _runtime.EnterLevelComplete();
    }
}
