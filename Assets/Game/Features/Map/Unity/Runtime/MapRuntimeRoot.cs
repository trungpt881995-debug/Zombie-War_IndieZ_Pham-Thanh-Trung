using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Services;
using ZombieWar.Features.Map.Unity.Config;

namespace ZombieWar.Features.Map.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class MapRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private MapCatalogConfig catalogConfig;
        [SerializeField] private Transform mapRoot;
        [SerializeField] private MapAssetLoaderBehaviour assetLoader;

        private IMapRuntime _runtime;
        private IMapRuntimeConfigurator _configurator;
        private CancellationTokenSource _lifetimeCts;

        public bool IsInitialized { get; private set; }
        public IMapRuntime Runtime => _runtime;

        public void Initialize(IMapRuntime runtime, IMapRuntimeConfigurator configurator)
        {
            if (IsInitialized) return;
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            if (catalogConfig == null) throw new InvalidOperationException("MapCatalogConfig is not assigned.");
            if (mapRoot == null) throw new InvalidOperationException("MapRoot Transform is not assigned.");
            if (assetLoader == null) throw new InvalidOperationException("MapAssetLoaderBehaviour is not assigned.");

            assetLoader.SetParent(mapRoot);
            _configurator.Initialize(catalogConfig.CreateCatalog(), assetLoader);
            _lifetimeCts = new CancellationTokenSource();
            IsInitialized = true;
        }

        public Task<MapLoadResult> LoadMapAsync(MapId mapId)
        {
            return _runtime != null
                ? _runtime.LoadAsync(mapId, _lifetimeCts != null ? _lifetimeCts.Token : CancellationToken.None)
                : Task.FromResult(MapLoadResult.Failed(mapId, MapLoadFailureReason.NotInitialized, "MapRuntimeRoot is not initialized."));
        }

        public Task UnloadMapAsync()
        {
            return _runtime != null
                ? _runtime.UnloadAsync(_lifetimeCts != null ? _lifetimeCts.Token : CancellationToken.None)
                : Task.CompletedTask;
        }

        private async void OnDestroy()
        {
            if (!IsInitialized) return;
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();
            _lifetimeCts = null;
            try { await _configurator.ShutdownAsync(CancellationToken.None); }
            catch (Exception ex) { Debug.LogException(ex); }
            IsInitialized = false;
        }
    }
}
