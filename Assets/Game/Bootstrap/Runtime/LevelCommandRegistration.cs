using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Level.Commands;
namespace ZombieWar.Bootstrap
{
    public sealed class LevelCommandRegistration:IStartable
    {
        private readonly ICommandRegistry _commands; 
        private readonly BeginGameLevelCommandHandler _begin; 
        private readonly RegisterNormalZombieKillCommandHandler _kill; 
        private readonly RegisterBossDefeatedCommandHandler _boss; 
        private readonly SetLevelProgressionEnabledCommandHandler _enabled;
        public LevelCommandRegistration(ICommandRegistry commands,BeginGameLevelCommandHandler begin,RegisterNormalZombieKillCommandHandler kill,RegisterBossDefeatedCommandHandler boss,SetLevelProgressionEnabledCommandHandler enabled)
        {
            _commands=commands;
            _begin=begin;_kill=kill;
            _boss=boss;_enabled=enabled;
        }
        public void Start()
        {
            _commands.Register<BeginGameLevelCommand>(_begin);
            _commands.Register<RegisterNormalZombieKillCommand>(_kill);
            _commands.Register<RegisterBossDefeatedCommand>(_boss);
            _commands.Register<SetLevelProgressionEnabledCommand>(_enabled);
        }
    }
}
