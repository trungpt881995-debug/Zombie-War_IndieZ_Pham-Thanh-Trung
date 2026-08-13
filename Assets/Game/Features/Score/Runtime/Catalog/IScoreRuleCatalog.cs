using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Rules;

namespace ZombieWar.Features.Score.Catalog
{
    public interface IScoreRuleCatalog
    {
        int Count { get; }
        bool TryGet(ScoreActionId actionId, out IScoreRule rule);
    }
}
