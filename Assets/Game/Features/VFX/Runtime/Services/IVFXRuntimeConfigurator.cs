using ZombieWar.Features.VFX.Catalog; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Services { public interface IVFXRuntimeConfigurator { void Initialize(IVFXCatalog catalog,IVFXPoolRegistry pools); void Shutdown(); } }
