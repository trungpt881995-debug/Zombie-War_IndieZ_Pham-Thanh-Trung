using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Score.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class ScoreCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands;
        private readonly StartScoreRunCommandHandler _start;
        private readonly BeginScoreLevelCommandHandler _begin;
        private readonly ReplayScoreLevelCommandHandler _replay;
        private readonly AwardScoreCommandHandler _award;
        private readonly SetScoringEnabledCommandHandler _enabled;

        public ScoreCommandRegistration(
            ICommandRegistry commands,
            StartScoreRunCommandHandler start,
            BeginScoreLevelCommandHandler begin,
            ReplayScoreLevelCommandHandler replay,
            AwardScoreCommandHandler award,
            SetScoringEnabledCommandHandler enabled)
        {
            _commands = commands;
            _start = start;
            _begin = begin;
            _replay = replay;
            _award = award;
            _enabled = enabled;
        }

        public void Start()
        {
            _commands.Register<StartScoreRunCommand>(_start);
            _commands.Register<BeginScoreLevelCommand>(_begin);
            _commands.Register<ReplayScoreLevelCommand>(_replay);
            _commands.Register<AwardScoreCommand>(_award);
            _commands.Register<SetScoringEnabledCommand>(_enabled);
        }
    }
}
