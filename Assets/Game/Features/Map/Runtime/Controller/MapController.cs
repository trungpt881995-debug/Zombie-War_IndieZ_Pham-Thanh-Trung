using System;
using System.Threading;
using System.Threading.Tasks;
using GeneralCore.Architecture;
using ZombieWar.Features.Map.Catalog;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Events;
using ZombieWar.Features.Map.Model;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Controller
{
    public sealed class MapController
    {
        private readonly MapModel _model;
        private readonly IMapCatalog _catalog;
        private readonly IMapAssetLoader _loader;
        private readonly IEventBus _events;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly object _operationSync = new object();

        private CancellationTokenSource _activeOperationCts;
        private IMapInstance _currentInstance;
        private int _operationVersion;

        public MapController(MapModel model, IMapCatalog catalog, IMapAssetLoader loader, IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public async Task<MapLoadResult> LoadAsync(MapId mapId, CancellationToken cancellationToken)
        {
            if (mapId == MapId.None)
                return MapLoadResult.Failed(mapId, MapLoadFailureReason.InvalidMapId, "MapId.None cannot be loaded.");

            int version = BeginOperation(out CancellationToken operationToken);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, operationToken);
            CancellationToken token = linked.Token;

            try
            {
                await _gate.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                return MapLoadResult.Failed(mapId, MapLoadFailureReason.Cancelled, "Map load was cancelled before it started.");
            }

            try
            {
                if (!IsCurrentOperation(version) || token.IsCancellationRequested)
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.Cancelled, "Map load was superseded.");

                if (_model.State == MapState.Loaded && _model.CurrentMapId == mapId && _currentInstance != null)
                    return MapLoadResult.Already(mapId);

                if (!_catalog.TryGet(mapId, out MapDefinition definition))
                {
                    // A bad replacement request must not destroy a currently valid loaded map.
                    if (_currentInstance == null) _model.SetFailed();
                    PublishLoadFailed(mapId, MapLoadFailureReason.MissingDefinition, $"Map definition not found: {mapId}.");
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.MissingDefinition, $"Map definition not found: {mapId}.");
                }

                if (_currentInstance != null)
                    await UnloadCurrentUnderGateAsync(CancellationToken.None);

                if (!IsCurrentOperation(version) || token.IsCancellationRequested)
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.Cancelled, "Map load was superseded before asset loading.");

                _model.BeginOperation();
                _model.BeginLoading();
                _events.Publish(new MapLoadStartedEvent(mapId));

                IMapInstance instance = null;
                try
                {
                    instance = await _loader.LoadAsync(definition, token);
                }
                catch (OperationCanceledException)
                {
                    if (instance != null) await SafeReleaseAsync(instance);
                    if (IsCurrentOperation(version)) _model.SetUnloaded();
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.Cancelled, "Map asset loading was cancelled.");
                }
                catch (Exception ex)
                {
                    if (instance != null) await SafeReleaseAsync(instance);
                    if (IsCurrentOperation(version))
                    {
                        _model.SetFailed();
                        PublishLoadFailed(mapId, MapLoadFailureReason.LoaderFailed, ex.Message);
                    }
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.LoaderFailed, ex.Message);
                }

                if (!IsCurrentOperation(version) || token.IsCancellationRequested)
                {
                    if (instance != null) await SafeReleaseAsync(instance);
                    if (IsCurrentOperation(version)) _model.SetUnloaded();
                    return MapLoadResult.Failed(mapId, MapLoadFailureReason.Cancelled, "Stale map load completion was discarded.");
                }

                if (instance == null)
                    return await FailAndReleaseAsync(mapId, null, MapLoadFailureReason.InvalidMapInstance, "Map loader returned null.");

                if (instance.MapId != mapId)
                    return await FailAndReleaseAsync(mapId, instance, MapLoadFailureReason.InvalidMapInstance, $"Loaded instance id {instance.MapId} does not match requested id {mapId}.");

                IMapView view = instance.View;
                if (view == null || view.Id != mapId)
                    return await FailAndReleaseAsync(mapId, instance, MapLoadFailureReason.InvalidMapView, "Loaded map view is missing or has the wrong MapId.");

                MapRuntimeContext context;
                string contextError;
                try
                {
                    if (!view.TryBuildContext(out context, out contextError) || context == null)
                        return await FailAndReleaseAsync(mapId, instance, MapLoadFailureReason.InvalidRuntimeContext, contextError);
                }
                catch (Exception ex)
                {
                    return await FailAndReleaseAsync(mapId, instance, MapLoadFailureReason.InvalidRuntimeContext, ex.Message);
                }

                if (context.MapId != mapId)
                    return await FailAndReleaseAsync(mapId, instance, MapLoadFailureReason.InvalidRuntimeContext, "Map runtime context MapId does not match the requested map.");

                _currentInstance = instance;
                _model.SetLoaded(mapId, context);
                _events.Publish(new MapLoadedEvent(mapId));
                return MapLoadResult.Loaded(mapId);
            }
            finally
            {
                _gate.Release();
                EndOperation(version);
            }
        }

        public async Task UnloadAsync(CancellationToken cancellationToken)
        {
            int version = BeginOperation(out CancellationToken operationToken);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, operationToken);
            CancellationToken token = linked.Token;

            try
            {
                await _gate.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                if (!IsCurrentOperation(version) || token.IsCancellationRequested) return;
                await UnloadCurrentUnderGateAsync(CancellationToken.None);
                if (_currentInstance == null && _model.State != MapState.Failed) _model.SetUnloaded();
            }
            finally
            {
                _gate.Release();
                EndOperation(version);
            }
        }

        private async Task<MapLoadResult> FailAndReleaseAsync(MapId mapId, IMapInstance instance, MapLoadFailureReason reason, string message)
        {
            if (instance != null) await SafeReleaseAsync(instance);
            _model.SetFailed();
            PublishLoadFailed(mapId, reason, message);
            return MapLoadResult.Failed(mapId, reason, message);
        }

        private async Task UnloadCurrentUnderGateAsync(CancellationToken cancellationToken)
        {
            IMapInstance instance = _currentInstance;
            MapId previousMapId = _model.CurrentMapId;

            if (instance == null)
            {
                _model.SetUnloaded();
                return;
            }

            _model.BeginUnloading();
            _events.Publish(new MapUnloadStartedEvent(previousMapId));
            _currentInstance = null;

            try
            {
                await _loader.ReleaseAsync(instance, cancellationToken);
            }
            finally
            {
                _model.SetUnloaded();
                _events.Publish(new MapUnloadedEvent(previousMapId));
            }
        }

        private async Task SafeReleaseAsync(IMapInstance instance)
        {
            try { await _loader.ReleaseAsync(instance, CancellationToken.None); }
            catch { /* Cleanup is best-effort; primary load failure remains authoritative. */ }
        }

        private int BeginOperation(out CancellationToken operationToken)
        {
            lock (_operationSync)
            {
                _activeOperationCts?.Cancel();
                _activeOperationCts?.Dispose();
                _activeOperationCts = new CancellationTokenSource();
                _operationVersion++;
                operationToken = _activeOperationCts.Token;
                return _operationVersion;
            }
        }

        private bool IsCurrentOperation(int version)
        {
            lock (_operationSync) return version == _operationVersion;
        }

        private void EndOperation(int version)
        {
            lock (_operationSync)
            {
                if (version != _operationVersion) return;
                _activeOperationCts?.Dispose();
                _activeOperationCts = null;
            }
        }

        private void PublishLoadFailed(MapId mapId, MapLoadFailureReason reason, string message)
        {
            _events.Publish(new MapLoadFailedEvent(mapId, reason, message));
        }
    }
}
