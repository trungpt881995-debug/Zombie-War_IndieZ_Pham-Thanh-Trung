using System;
using System.Collections.Generic;

namespace GameplayCore.Save
{
    [Serializable]
    public sealed class GameplaySaveSnapshot
    {
        private readonly Dictionary<string, string> _featurePayloads = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> FeaturePayloads => _featurePayloads;
        public void Set(string featureId, string payload) => _featurePayloads[featureId] = payload ?? string.Empty;
        public bool TryGet(string featureId, out string payload) => _featurePayloads.TryGetValue(featureId, out payload);
    }

    public interface IGameplaySaveContributor
    {
        string FeatureId { get; }
        void Capture(GameplaySaveSnapshot snapshot);
        void Restore(GameplaySaveSnapshot snapshot);
    }
}
