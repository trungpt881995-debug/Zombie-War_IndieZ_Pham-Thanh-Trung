using System;
using System.Collections.Generic;
using ZombieWar.Features.Level.Domain;

namespace ZombieWar.Features.Level.Catalog
{
    public sealed class LevelCatalog : ILevelCatalog
    {
        private readonly Dictionary < GameLevelId, LevelDefinition > _levels;
        public int Count => _levels.Count;
        public LevelCatalog(LevelDefinition[] definitions)
        {
            if (definitions == null || definitions.Length == 0) throw new ArgumentException("At least one level definition is required.",
            nameof(definitions));
            _levels = new Dictionary < GameLevelId, LevelDefinition > (definitions.Length);
            int finalCount = 0;
            for (int i = 0; i < definitions.Length; i++)
            {
                var d = definitions[i] ?? throw new ArgumentException($"Level definition at {i} is null.", nameof(definitions));
                if (!_levels.TryAdd(d.Id, d)) throw new ArgumentException($"Duplicate Game Level: {d.Id}.", nameof(definitions));
                if (d.IsFinalLevel) finalCount++;
            }
            if (finalCount != 1) throw new ArgumentException("Exactly one final Game Level is required.", nameof(definitions));
        }
        public bool TryGet(GameLevelId id, out LevelDefinition definition)
        {
            definition = null;
            if (id == GameLevelId.None) return false;
            return _levels.TryGetValue(id, out definition);
        }
    }
}
