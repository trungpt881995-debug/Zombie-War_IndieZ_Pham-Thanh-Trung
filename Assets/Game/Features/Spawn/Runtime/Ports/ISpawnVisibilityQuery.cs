using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Ports 
{ 
  public interface ISpawnVisibilityQuery 
  { 
    bool IsVisible(in SpawnPoint point); 
  } 
}
