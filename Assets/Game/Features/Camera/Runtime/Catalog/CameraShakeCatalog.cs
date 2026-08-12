using System;
using System.Collections.Generic;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Catalog
{
    public sealed class CameraShakeCatalog : ICameraShakeCatalog
    {
        private readonly Dictionary<CameraShakeId, CameraShakeDefinition> _definitions;

        public CameraShakeCatalog(IReadOnlyList<CameraShakeDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            _definitions = new Dictionary<CameraShakeId, CameraShakeDefinition>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                CameraShakeDefinition definition = definitions[i];
                if (_definitions.ContainsKey(definition.Id))
                    throw new ArgumentException($"Duplicate camera shake id: {definition.Id}.", nameof(definitions));
                _definitions.Add(definition.Id, definition);
            }
        }

        public bool TryGet(CameraShakeId id, out CameraShakeDefinition definition)
        {
            definition = default;

            if (id == CameraShakeId.None)
                return false;

            return _definitions.TryGetValue(id, out definition);
        }
    }
}
