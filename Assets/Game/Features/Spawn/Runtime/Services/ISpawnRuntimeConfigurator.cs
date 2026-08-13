using ZombieWar.Features.Spawn.Catalog; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports; using ZombieWar.Features.Spawn.Strategies;
namespace ZombieWar.Features.Spawn.Services
{
    public interface ISpawnRuntimeConfigurator
    {
        void Initialize(in SpawnDifficultyKey initialDifficulty,ISpawnTuningCatalog catalog,ISpawnRandom random,ISpawnSectorProvider sectorProvider,ISpawnVisibilityQuery visibilityQuery,ISpawnGameplayBoundsQuery gameplayBoundsQuery,ISpawnNavigationQuery navigationQuery,IZombieSpawnPort zombieSpawnPort,IZombiePopulationQuery populationQuery,ISpawnSectorSelectionStrategy sectorSelectionStrategy,ISpawnPositionStrategy positionStrategy,int maxPlacementAttempts);
        void Shutdown();
    }
}
