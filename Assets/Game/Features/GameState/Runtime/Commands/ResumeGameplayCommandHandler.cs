using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Services;
namespace ZombieWar.Features.GameState.Commands
{
    public sealed class ResumeGameplayCommandHandler : ICommandHandler<ResumeGameplayCommand>
    {
        private readonly IGameStateRuntime _runtime;
        public ResumeGameplayCommandHandler(IGameStateRuntime runtime) => _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(ResumeGameplayCommand command) => _runtime.Resume();
    }
}
