using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Features.Spawn.Strategies { public interface ISpawnSectorSelectionStrategy { SpawnSectorId Select(ISpawnRandom random); } }
