using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Catalog
{
    public interface IBossCatalog
    {
        bool TryGet(BossId id, out BossDefinition definition);
    }
}
