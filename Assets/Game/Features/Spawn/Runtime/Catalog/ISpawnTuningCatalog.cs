using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Catalog { public interface ISpawnTuningCatalog { bool TryGet(in SpawnDifficultyKey key,out SpawnTuning tuning); } }
