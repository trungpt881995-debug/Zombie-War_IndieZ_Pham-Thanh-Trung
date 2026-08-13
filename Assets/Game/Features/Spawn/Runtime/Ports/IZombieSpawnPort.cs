using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Ports { public interface IZombieSpawnPort { bool TrySpawn(in SpawnPoint position); } }
