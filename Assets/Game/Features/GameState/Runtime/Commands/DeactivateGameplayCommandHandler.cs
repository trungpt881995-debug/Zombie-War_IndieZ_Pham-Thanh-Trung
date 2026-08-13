using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class DeactivateGameplayCommandHandler : ICommandHandler<DeactivateGameplayCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public DeactivateGameplayCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(DeactivateGameplayCommand command) => _runtime.Deactivate();
    }
}
