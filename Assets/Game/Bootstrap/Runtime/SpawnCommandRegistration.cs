using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Spawn.Commands;
namespace ZombieWar.Bootstrap
{
    public sealed class SpawnCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands; 
        private readonly StartZombieSpawningCommandHandler _start; 
        private readonly StopZombieSpawningCommandHandler _stop; 
        private readonly SetSpawnDifficultyCommandHandler _difficulty;
        public SpawnCommandRegistration(ICommandRegistry commands,StartZombieSpawningCommandHandler start,StopZombieSpawningCommandHandler stop,SetSpawnDifficultyCommandHandler difficulty)
        {
            _commands=commands;
            _start=start;
            _stop=stop;
            _difficulty=difficulty;
        }
        public void Start()
        {
            _commands.Register<StartZombieSpawningCommand>(_start);
            _commands.Register<StopZombieSpawningCommand>(_stop);
            _commands.Register<SetSpawnDifficultyCommand>(_difficulty);
        }
    }
}
