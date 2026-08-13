using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Ports { public interface IVFXPoolRegistry { bool TryAcquire(VFXId id,out IVFXLease lease); void ReleaseAll(); } }
