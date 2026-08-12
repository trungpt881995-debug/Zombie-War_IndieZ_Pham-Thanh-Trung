using System;
using UnityEngine;
using ZombieWar.Features.Map.Catalog;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Unity.Config
{
    [CreateAssetMenu(menuName = "Zombie War/Map/Map Catalog Config", fileName = "MapCatalogConfig")]
    public sealed class MapCatalogConfig : ScriptableObject
    {
        [SerializeField] private MapConfig[] maps = new MapConfig[2];

        public IMapCatalog CreateCatalog()
        {
            if (maps == null || maps.Length != 2)
                throw new InvalidOperationException("Zombie War requires exactly two MapConfig assets.");

            var definitions = new MapDefinition[maps.Length];
            var seen = new bool[3];
            for (int i = 0; i < maps.Length; i++)
            {
                MapConfig config = maps[i];
                if (config == null) throw new InvalidOperationException($"MapConfig at index {i} is not assigned.");
                int id = (int)config.MapId;
                if (id <= 0 || id >= seen.Length) throw new InvalidOperationException($"Unsupported MapId: {config.MapId}.");
                if (seen[id]) throw new InvalidOperationException($"Duplicate MapId: {config.MapId}.");
                seen[id] = true;
                definitions[i] = config.CreateDefinition();
            }

            if (!seen[(int)MapId.Map01] || !seen[(int)MapId.Map02])
                throw new InvalidOperationException("MapCatalogConfig must contain Map01 and Map02 exactly once.");

            return new MapCatalog(definitions);
        }

        public bool TryGetDevelopmentPrefab(MapId mapId, out GameObject prefab)
        {
            if (maps != null)
            {
                for (int i = 0; i < maps.Length; i++)
                {
                    MapConfig config = maps[i];
                    if (config == null || config.MapId != mapId) continue;
                    prefab = config.DevelopmentPrefab;
                    return prefab != null;
                }
            }

            prefab = null;
            return false;
        }
    }
}
