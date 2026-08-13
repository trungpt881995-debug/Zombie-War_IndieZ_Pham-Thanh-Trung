using GeneralCore.Architecture;
namespace ZombieWar.Features.Score.Commands { public readonly struct SetScoringEnabledCommand : ICommand { public bool Enabled { get; } public SetScoringEnabledCommand(bool enabled)=>Enabled=enabled; } }
