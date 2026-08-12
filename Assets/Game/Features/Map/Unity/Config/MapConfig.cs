using UnityEngine;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Unity.Config
{
    [CreateAssetMenu(menuName = "Zombie War/Map/Map Config", fileName = "MapConfig")]
    public sealed class MapConfig : ScriptableObject
    {
        [SerializeField] private MapId mapId = MapId.Map01;
        [SerializeField] private string addressableKey = "Map01";
        [Tooltip("Development/test fallback only. Production should load the same prefab through Addressables.")]
        [SerializeField] private GameObject developmentPrefab;

        public MapId MapId => mapId;
        public string AddressableKey => addressableKey;
        public GameObject DevelopmentPrefab => developmentPrefab;

        public MapDefinition CreateDefinition() => new MapDefinition(mapId, addressableKey);
    }
}
