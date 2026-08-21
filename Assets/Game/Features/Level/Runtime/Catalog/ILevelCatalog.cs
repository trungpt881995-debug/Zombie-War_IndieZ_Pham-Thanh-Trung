using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Catalog
{
    public interface ILevelCatalog
    {
        bool TryGet(GameLevelId id, out LevelDefinition definition);
    }
}
