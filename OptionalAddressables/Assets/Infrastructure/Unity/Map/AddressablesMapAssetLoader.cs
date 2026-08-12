using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Ports;
using ZombieWar.Features.Map.Unity.Authoring;
using ZombieWar.Features.Map.Unity.Runtime;

namespace ZombieWar.Infrastructure.Map.Addressables
{
    [DisallowMultipleComponent]
    public sealed class AddressablesMapAssetLoader : MapAssetLoaderBehaviour
    {
        public override async Task<IMapInstance> LoadAsync(MapDefinition definition, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(definition.AssetKey, Parent);
            GameObject gameObject = null;
            try
            {
                gameObject = await handle.Task;
                cancellationToken.ThrowIfCancellationRequested();
                if (handle.Status != AsyncOperationStatus.Succeeded || gameObject == null)
                    throw new InvalidOperationException($"Addressables failed to instantiate map '{definition.AssetKey}'.");

                MapView view = gameObject.GetComponent<MapView>();
                return new AddressablesMapInstance(definition.Id, gameObject, view);
            }
            catch
            {
                if (gameObject != null) Addressables.ReleaseInstance(gameObject);
                else if (handle.IsValid()) Addressables.Release(handle);
                throw;
            }
        }

        public override Task ReleaseAsync(IMapInstance instance, CancellationToken cancellationToken)
        {
            if (instance is AddressablesMapInstance addressableInstance && addressableInstance.GameObject != null)
                Addressables.ReleaseInstance(addressableInstance.GameObject);
            return Task.CompletedTask;
        }

        private sealed class AddressablesMapInstance : IMapInstance
        {
            public MapId MapId { get; }
            public IMapView View { get; }
            public GameObject GameObject { get; }

            public AddressablesMapInstance(MapId mapId, GameObject gameObject, IMapView view)
            {
                MapId = mapId;
                GameObject = gameObject;
                View = view;
            }
        }
    }
}
