using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Catalog
{
    public interface IMapCatalog
    {
        bool TryGet(MapId mapId, out MapDefinition definition);
    }
}
