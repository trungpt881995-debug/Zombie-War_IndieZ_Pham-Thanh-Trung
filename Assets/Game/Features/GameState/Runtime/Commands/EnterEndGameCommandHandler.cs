using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class EnterEndGameCommandHandler : ICommandHandler<EnterEndGameCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public EnterEndGameCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(EnterEndGameCommand command) => _runtime.EnterEndGame();
    }
}
