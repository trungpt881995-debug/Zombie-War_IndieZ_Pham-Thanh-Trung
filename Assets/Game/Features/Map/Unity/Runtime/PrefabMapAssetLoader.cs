using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Ports;
using ZombieWar.Features.Map.Unity.Authoring;
using ZombieWar.Features.Map.Unity.Config;

namespace ZombieWar.Features.Map.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PrefabMapAssetLoader : MapAssetLoaderBehaviour
    {
        [SerializeField] private MapCatalogConfig catalogConfig;

        public override Task<IMapInstance> LoadAsync(MapDefinition definition, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (catalogConfig == null) throw new InvalidOperationException("MapCatalogConfig is not assigned on PrefabMapAssetLoader.");
            if (!catalogConfig.TryGetDevelopmentPrefab(definition.Id, out GameObject prefab) || prefab == null)
                throw new InvalidOperationException($"Development prefab is not assigned for {definition.Id}.");

            GameObject instanceObject = Instantiate(prefab, Parent);
            MapView view = instanceObject.GetComponent<MapView>();
            return Task.FromResult<IMapInstance>(new PrefabMapInstance(definition.Id, instanceObject, view));
        }

        public override Task ReleaseAsync(IMapInstance instance, CancellationToken cancellationToken)
        {
            if (instance is PrefabMapInstance prefabInstance && prefabInstance.GameObject != null)
                Destroy(prefabInstance.GameObject);
            return Task.CompletedTask;
        }

        private sealed class PrefabMapInstance : IMapInstance
        {
            public MapId MapId { get; }
            public IMapView View { get; }
            public GameObject GameObject { get; }

            public PrefabMapInstance(MapId mapId, GameObject gameObject, IMapView view)
            {
                MapId = mapId;
                GameObject = gameObject;
                View = view;
            }
        }
    }
}
