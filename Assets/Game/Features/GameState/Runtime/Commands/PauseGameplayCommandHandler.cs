using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class PauseGameplayCommandHandler : ICommandHandler<PauseGameplayCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public PauseGameplayCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(PauseGameplayCommand command) => _runtime.Pause();
    }
}
