using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Catalog { public interface IVFXCatalog { int Count{get;} bool TryGet(VFXId id,out VFXDefinition definition); } }
