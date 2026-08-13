using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Ports { public interface ISpawnSectorProvider { bool TryGetSector(SpawnSectorId id,out SpawnSector sector); } }
