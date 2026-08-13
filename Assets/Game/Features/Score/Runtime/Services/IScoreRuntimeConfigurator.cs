using ZombieWar.Features.Score.Catalog;

namespace ZombieWar.Features.Score.Services
{
    public interface IScoreRuntimeConfigurator
    {
        void Initialize(IScoreRuleCatalog catalog);
        void Shutdown();
    }
}
