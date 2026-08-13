using System;
using System.Collections.Generic;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Catalog
{
    public sealed class FeedbackCatalog : IFeedbackCatalog
    {
        private readonly Dictionary<FeedbackId, FeedbackRecipe> _recipes;

        public FeedbackCatalog(IReadOnlyList<FeedbackRecipe> recipes)
        {
            if (recipes == null)
            {
                throw new ArgumentNullException(nameof(recipes));
            }

            _recipes = new Dictionary<FeedbackId, FeedbackRecipe>(recipes.Count);

            for (int i = 0; i < recipes.Count; i++)
            {
                FeedbackRecipe recipe = recipes[i];

                if (_recipes.ContainsKey(recipe.Id))
                {
                    throw new InvalidOperationException(
                        $"Duplicate FeedbackId in catalog: {recipe.Id}");
                }

                _recipes.Add(recipe.Id, recipe);
            }
        }

        public int Count => _recipes.Count;

        public bool TryGet(
            FeedbackId id,
            out FeedbackRecipe recipe)
        {
            return _recipes.TryGetValue(id, out recipe);
        }
    }
}
