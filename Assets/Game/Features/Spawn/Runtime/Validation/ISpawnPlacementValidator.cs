using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Validation 
{ 
  public interface ISpawnPlacementValidator 
  { 
    SpawnPlacementResult Validate(in SpawnPoint candidate); 
  } 
}
