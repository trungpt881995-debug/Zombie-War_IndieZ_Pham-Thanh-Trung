using System;
using System.Collections.Generic;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Catalog
{
    public sealed class MapCatalog : IMapCatalog
    {
        private readonly Dictionary<MapId, MapDefinition> _definitions;

        public MapCatalog(IReadOnlyList<MapDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            _definitions = new Dictionary<MapId, MapDefinition>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                MapDefinition definition = definitions[i];
                if (_definitions.ContainsKey(definition.Id))
                    throw new ArgumentException($"Duplicate MapId: {definition.Id}.", nameof(definitions));
                _definitions.Add(definition.Id, definition);
            }
        }

        public bool TryGet(MapId mapId, out MapDefinition definition) => _definitions.TryGetValue(mapId, out definition);
    }
}
