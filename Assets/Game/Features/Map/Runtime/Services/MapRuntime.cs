using System;
using System.Threading;
using System.Threading.Tasks;
using GeneralCore.Architecture;
using ZombieWar.Features.Map.Catalog;
using ZombieWar.Features.Map.Controller;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Model;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Services
{
    public sealed class MapRuntime : IMapRuntime, IMapRuntimeConfigurator
    {
        private readonly IEventBus _events;
        private MapModel _model;
        private MapController _controller;

        public bool IsInitialized => _controller != null;
        public MapState State => IsInitialized ? _model.State : MapState.Unloaded;
        public MapId CurrentMapId => IsInitialized ? _model.CurrentMapId : MapId.None;
        public MapRuntimeContext CurrentContext => IsInitialized ? _model.Context : null;

        public MapRuntime(IEventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Initialize(IMapCatalog catalog, IMapAssetLoader assetLoader)
        {
            if (IsInitialized) throw new InvalidOperationException("MapRuntime is already initialized.");
            _model = new MapModel();
            _controller = new MapController(_model, catalog, assetLoader, _events);
        }

        public Task<MapLoadResult> LoadAsync(MapId mapId, CancellationToken cancellationToken)
        {
            return IsInitialized
                ? _controller.LoadAsync(mapId, cancellationToken)
                : Task.FromResult(MapLoadResult.Failed(mapId, MapLoadFailureReason.NotInitialized, "MapRuntime is not initialized."));
        }

        public Task UnloadAsync(CancellationToken cancellationToken)
        {
            return IsInitialized ? _controller.UnloadAsync(cancellationToken) : Task.CompletedTask;
        }

        public bool TryGetCurrentContext(out MapRuntimeContext context)
        {
            context = CurrentContext;
            return context != null && State == MapState.Loaded;
        }

        public async Task ShutdownAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized) return;
            await _controller.UnloadAsync(cancellationToken);
            _controller = null;
            _model = null;
        }
    }
}
