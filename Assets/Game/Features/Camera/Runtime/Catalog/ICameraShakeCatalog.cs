using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Catalog
{
    public interface ICameraShakeCatalog
    {
        bool TryGet(CameraShakeId id, out CameraShakeDefinition definition);
    }
}
