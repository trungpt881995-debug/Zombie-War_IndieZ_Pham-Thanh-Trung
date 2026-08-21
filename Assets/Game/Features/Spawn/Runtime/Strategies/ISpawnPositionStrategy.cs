using ZombieWar.Features.Spawn.Domain; 
using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Features.Spawn.Strategies 
{ 
  public interface ISpawnPositionStrategy 
  { 
    SpawnPoint Select(in SpawnArea area,ISpawnRandom random); 
  } 
}
