using System;
using System.Collections.Generic;
using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Catalog
{
    public interface IAudioCatalog
    {
        int Count { get; }

        bool TryGet(
            AudioId id,
            out AudioDefinition definition);
    }

    public sealed class AudioCatalog : IAudioCatalog
    {
        private readonly Dictionary<AudioId, AudioDefinition> _definitions;

        public AudioCatalog(IEnumerable<AudioDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _definitions = new Dictionary<AudioId, AudioDefinition>();

            foreach (AudioDefinition definition in definitions)
            {
                if (_definitions.ContainsKey(definition.Id))
                {
                    throw new InvalidOperationException(
                        $"Duplicate AudioId in catalog: {definition.Id}.");
                }

                _definitions.Add(
                    definition.Id,
                    definition);
            }
        }

        public int Count => _definitions.Count;

        public bool TryGet(
            AudioId id,
            out AudioDefinition definition)
        {
            return _definitions.TryGetValue(
                id,
                out definition);
        }
    }
}
