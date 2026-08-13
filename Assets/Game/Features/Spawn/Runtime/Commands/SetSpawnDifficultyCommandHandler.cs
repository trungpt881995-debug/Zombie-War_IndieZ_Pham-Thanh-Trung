using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Spawn.Domain;
using ZombieWar.Features.Spawn.Services;

namespace ZombieWar.Features.Spawn.Commands
{
    public sealed class SetSpawnDifficultyCommandHandler : ICommandHandler<SetSpawnDifficultyCommand>
    {
        private readonly ISpawnRuntime _runtime;

        public SetSpawnDifficultyCommandHandler(ISpawnRuntime runtime) =>
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

        public void Handle(SetSpawnDifficultyCommand command)
        {
            SpawnDifficultyKey key = command.Key;
            _runtime.SetDifficulty(in key);
        }
    }
}
