using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Features.Score.Rules
{
    public interface IScoreRule
    {
        ScoreActionId ActionId { get; }
        long Calculate(in ScoreContext context);
    }
}
