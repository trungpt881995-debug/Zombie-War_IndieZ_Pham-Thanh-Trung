using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class EnterGameOverCommandHandler : ICommandHandler<EnterGameOverCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public EnterGameOverCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(EnterGameOverCommand command) => _runtime.EnterGameOver();
    }
}
