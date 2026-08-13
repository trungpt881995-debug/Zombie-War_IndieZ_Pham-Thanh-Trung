using ZombieWar.Features.Level.Catalog; namespace ZombieWar.Features.Level.Services { public interface ILevelRuntimeConfigurator { void Initialize(ILevelCatalog catalog); void Shutdown(); } }
