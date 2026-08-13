using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Catalog
{
    public interface IFeedbackCatalog
    {
        int Count { get; }

        bool TryGet(
            FeedbackId id,
            out FeedbackRecipe recipe);
    }
}
