using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class BeginGameplayCommandHandler : ICommandHandler<BeginGameplayCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public BeginGameplayCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(BeginGameplayCommand command) => _runtime.BeginGameplay();
    }
}
