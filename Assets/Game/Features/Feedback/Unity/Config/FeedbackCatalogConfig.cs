using System;
using UnityEngine;
using ZombieWar.Features.Feedback.Catalog;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Unity.Config
{
    [CreateAssetMenu(
        fileName = "FeedbackCatalog_Game",
        menuName = "Zombie War/Feedback/Feedback Catalog")]
    public sealed class FeedbackCatalogConfig : ScriptableObject
    {
        [SerializeField] private FeedbackConfig[] configs = Array.Empty<FeedbackConfig>();

        public IFeedbackCatalog CreateCatalog()
        {
            int count = configs == null ? 0 : configs.Length;
            var recipes = new FeedbackRecipe[count];

            for (int i = 0; i < count; i++)
            {
                if (configs[i] == null)
                {
                    throw new InvalidOperationException(
                        $"Feedback catalog contains a null config at index {i}.");
                }

                recipes[i] = configs[i].CreateRecipe();
            }

            return new FeedbackCatalog(recipes);
        }

#if UNITY_EDITOR
        public void EditorSetConfigs(FeedbackConfig[] value)
        {
            configs = value ?? Array.Empty<FeedbackConfig>();
        }
#endif
    }
}
