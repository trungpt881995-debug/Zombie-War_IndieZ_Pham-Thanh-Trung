using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Boss.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class BossCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands;
        private readonly SpawnLevelBossesCommandHandler _spawn;
        private readonly SetBossGameplayEnabledCommandHandler _enabled;
        private readonly CancelAllBossesCommandHandler _cancel;

        public BossCommandRegistration( ICommandRegistry commands, SpawnLevelBossesCommandHandler spawn, SetBossGameplayEnabledCommandHandler enabled, CancelAllBossesCommandHandler cancel)
        {
            _commands = commands;
            _spawn = spawn;
            _enabled = enabled;
            _cancel = cancel;
        }

        public void Start()
        {
            _commands.Register<SpawnLevelBossesCommand>(_spawn);
            _commands.Register<SetBossGameplayEnabledCommand>(_enabled);
            _commands.Register<CancelAllBossesCommand>(_cancel);
        }
    }
}
