using ZombieWar.Features.Boss.Catalog; using ZombieWar.Features.Boss.Ports; using ZombieWar.Features.Boss.Registry;
namespace ZombieWar.Features.Boss.Services { public interface IBossRuntimeConfigurator { void Initialize(IBossCatalog catalog,IBossSpawnExecutor spawnExecutor,IActiveBossRegistry registry); void Shutdown(); } }
