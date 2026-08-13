using GeneralCore.Architecture;
using ZombieWar.Features.Score.Domain;
namespace ZombieWar.Features.Score.Commands { public readonly struct BeginScoreLevelCommand : ICommand { public ScoreLevelId Level { get; } public BeginScoreLevelCommand(ScoreLevelId level) => Level = level; } }
