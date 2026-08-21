using GeneralCore.Architecture; 
using ZombieWar.Features.Spawn.Domain; 
namespace ZombieWar.Features.Spawn.Commands 
{ 
  public readonly struct SetSpawnDifficultyCommand : ICommand 
  { 
    public SpawnDifficultyKey Key { get; } 
    public SetSpawnDifficultyCommand(in SpawnDifficultyKey key)=>Key=key; 
  } 
}
