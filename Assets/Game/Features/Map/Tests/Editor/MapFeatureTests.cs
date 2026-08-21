using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeneralCore.Architecture;
using NUnit.Framework;
using ZombieWar.Features.Map.Catalog;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Events;
using ZombieWar.Features.Map.Ports;
using ZombieWar.Features.Map.Services;

namespace ZombieWar.Features.Map.Tests
{
    public sealed class MapFeatureTests
    {
        [Test] public void Bounds_Valid() { var b = new MapBounds(-1, 1, -2, 2); Assert.IsTrue(b.IsValid); }
        [Test] public void Bounds_Invalid_WhenZeroWidth() { var b = new MapBounds(1, 1, -2, 2); Assert.IsFalse(b.IsValid); }
        [TestCase(0f, 0f, true)]
        [TestCase(-1f, -2f, true)]
        [TestCase(1.1f, 0f, false)]
        [TestCase(0f, 2.1f, false)]
        public void Bounds_Contains(float x, float z, bool expected)
        {
            var b = new MapBounds(-1, 1, -2, 2);
            var p = new MapPoint(x, 0, z);
            Assert.AreEqual(expected, b.Contains(in p));
        }

        [Test] public void Definition_RejectsNone() => Assert.Throws<ArgumentOutOfRangeException>(() => new MapDefinition(MapId.None, "x"));
        [Test] public void Definition_RejectsEmptyKey() => Assert.Throws<ArgumentException>(() => new MapDefinition(MapId.Map01, ""));
        [Test] public void Catalog_RejectsDuplicateMap() => Assert.Throws<ArgumentException>(() => new MapCatalog(new[] { Def(MapId.Map01), Def(MapId.Map01) }));
        [Test] public void Catalog_FindsDefinition() { var c = Catalog(); Assert.IsTrue(c.TryGet(MapId.Map02, out var d)); Assert.AreEqual(MapId.Map02, d.Id); }

        [Test] public void Context_RequiresFourSectors() => Assert.Throws<ArgumentException>(() => new MapRuntimeContext(MapId.Map01, in ValidBounds, in ValidBounds, new MapSpawnSector[3], in Zero, false));
        [Test] public void Context_RejectsDuplicateSector() => Assert.Throws<ArgumentException>(() => new MapRuntimeContext(MapId.Map01, in ValidBounds, in ValidBounds, new[] { Sector(MapSpawnSectorId.Top), Sector(MapSpawnSectorId.Top), Sector(MapSpawnSectorId.Left), Sector(MapSpawnSectorId.Right) }, in Zero, false));
        [Test] public void Context_ExposesAllFourSectors() { var c = Context(MapId.Map01); Assert.AreEqual(4, c.SpawnSectors.Count); }
        [Test] public void Context_ExposesSoldierSpawnPoint() { var c = Context(MapId.Map01); Assert.IsTrue(c.HasSoldierSpawnPoint); Assert.AreEqual(SoldierSpawn, c.SoldierSpawnPoint); }
        [Test] public void Context_LegacyConstructor_DoesNotClaimSoldierSpawnPoint() { var c = new MapRuntimeContext(MapId.Map01, in ValidBounds, in ValidBounds, FourSectors(), in Zero, false); Assert.IsFalse(c.HasSoldierSpawnPoint); }
        [TestCase(MapSpawnSectorId.Top)]
        [TestCase(MapSpawnSectorId.Bottom)]
        [TestCase(MapSpawnSectorId.Left)]
        [TestCase(MapSpawnSectorId.Right)]
        public void Context_CanFindSector(MapSpawnSectorId id) { var c = Context(MapId.Map01); Assert.IsTrue(c.TryGetSpawnSector(id, out var s)); Assert.AreEqual(id, s.Id); }

        [Test] public async Task Runtime_NotInitialized_FailsGracefully()
        {
            var bus = new EventBus();
            var r = new MapRuntime(bus);
            MapLoadResult result = await r.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(MapLoadFailureReason.NotInitialized, result.FailureReason);
        }

        [Test] public async Task Runtime_LoadMap01_Succeeds()
        {
            var s = Setup();
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(MapState.Loaded, s.Runtime.State);
            Assert.AreEqual(MapId.Map01, s.Runtime.CurrentMapId);
            Assert.NotNull(s.Runtime.CurrentContext);
        }

        [Test] public async Task Runtime_SameMapLoad_IsIdempotent()
        {
            var s = Setup();
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            MapLoadResult second = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.IsTrue(second.Success);
            Assert.IsTrue(second.AlreadyLoaded);
            Assert.AreEqual(1, s.Loader.LoadCount);
        }

        [Test] public async Task Runtime_LoadUnknownMap_Fails()
        {
            var s = Setup(new MapCatalog(new[] { Def(MapId.Map01) }));
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map02, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.MissingDefinition, result.FailureReason);
            Assert.AreEqual(MapState.Failed, s.Runtime.State);
        }

        [Test] public async Task Runtime_InvalidReplacementRequest_PreservesCurrentLoadedMap()
        {
            var s = Setup(new MapCatalog(new[] { Def(MapId.Map01) }));
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map02, CancellationToken.None);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(MapId.Map01, s.Runtime.CurrentMapId);
            Assert.AreEqual(MapState.Loaded, s.Runtime.State);
            Assert.AreEqual(0, s.Loader.ReleaseCount);
        }

        [Test] public async Task Runtime_Unload_ClearsContext()
        {
            var s = Setup();
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            await s.Runtime.UnloadAsync(CancellationToken.None);
            Assert.AreEqual(MapState.Unloaded, s.Runtime.State);
            Assert.AreEqual(MapId.None, s.Runtime.CurrentMapId);
            Assert.IsNull(s.Runtime.CurrentContext);
            Assert.AreEqual(1, s.Loader.ReleaseCount);
        }

        [Test] public async Task Runtime_SwitchMap_ReleasesPrevious()
        {
            var s = Setup();
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            await s.Runtime.LoadAsync(MapId.Map02, CancellationToken.None);
            Assert.AreEqual(MapId.Map02, s.Runtime.CurrentMapId);
            Assert.AreEqual(2, s.Loader.LoadCount);
            Assert.AreEqual(1, s.Loader.ReleaseCount);
        }

        [Test] public async Task Runtime_LoaderException_SetsFailed()
        {
            var loader = new FakeLoader { ThrowOnLoad = true };
            var s = Setup(loader: loader);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.LoaderFailed, result.FailureReason);
            Assert.AreEqual(MapState.Failed, s.Runtime.State);
        }

        [Test] public async Task Runtime_NullInstance_Fails()
        {
            var loader = new FakeLoader { ReturnNull = true };
            var s = Setup(loader: loader);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.InvalidMapInstance, result.FailureReason);
        }

        [Test] public async Task Runtime_WrongInstanceId_FailsAndReleases()
        {
            var loader = new FakeLoader { ForceInstanceId = MapId.Map02 };
            var s = Setup(loader: loader);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.InvalidMapInstance, result.FailureReason);
            Assert.AreEqual(1, loader.ReleaseCount);
        }

        [Test] public async Task Runtime_MissingView_FailsAndReleases()
        {
            var loader = new FakeLoader { MissingView = true };
            var s = Setup(loader: loader);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.InvalidMapView, result.FailureReason);
            Assert.AreEqual(1, loader.ReleaseCount);
        }

        [Test] public async Task Runtime_InvalidContext_FailsAndReleases()
        {
            var loader = new FakeLoader { InvalidContext = true };
            var s = Setup(loader: loader);
            MapLoadResult result = await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(MapLoadFailureReason.InvalidRuntimeContext, result.FailureReason);
            Assert.AreEqual(1, loader.ReleaseCount);
        }

        [Test] public async Task Events_LoadLifecycle_Once()
        {
            var s = Setup();
            int started = 0, loaded = 0;
            using var a = s.Bus.Subscribe<MapLoadStartedEvent>(_ => started++);
            using var b = s.Bus.Subscribe<MapLoadedEvent>(_ => loaded++);
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(1, started);
            Assert.AreEqual(1, loaded);
        }

        [Test] public async Task Events_UnloadLifecycle_Once()
        {
            var s = Setup();
            int started = 0, unloaded = 0;
            using var a = s.Bus.Subscribe<MapUnloadStartedEvent>(_ => started++);
            using var b = s.Bus.Subscribe<MapUnloadedEvent>(_ => unloaded++);
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            await s.Runtime.UnloadAsync(CancellationToken.None);
            Assert.AreEqual(1, started);
            Assert.AreEqual(1, unloaded);
        }

        [Test] public async Task FailedLoad_DoesNotPublishLoaded()
        {
            var s = Setup(loader: new FakeLoader { ThrowOnLoad = true });
            int loaded = 0, failed = 0;
            using var a = s.Bus.Subscribe<MapLoadedEvent>(_ => loaded++);
            using var b = s.Bus.Subscribe<MapLoadFailedEvent>(_ => failed++);
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.AreEqual(0, loaded);
            Assert.AreEqual(1, failed);
        }

        [Test] public async Task Cancellation_ReturnsCancelled()
        {
            var loader = new FakeLoader { DelayLoad = true };
            var s = Setup(loader: loader);
            using var cts = new CancellationTokenSource();
            Task<MapLoadResult> task = s.Runtime.LoadAsync(MapId.Map01, cts.Token);
            await loader.LoadEntered.Task;
            cts.Cancel();
            loader.AllowLoad.TrySetResult(true);
            MapLoadResult result = await task;
            Assert.AreEqual(MapLoadFailureReason.Cancelled, result.FailureReason);
            Assert.AreNotEqual(MapState.Loaded, s.Runtime.State);
        }

        [Test] public async Task NewLoad_SupersedesOldLoad()
        {
            var loader = new FakeLoader { DelayFirstLoadOnly = true };
            var s = Setup(loader: loader);
            Task<MapLoadResult> first = s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            await loader.LoadEntered.Task;
            Task<MapLoadResult> second = s.Runtime.LoadAsync(MapId.Map02, CancellationToken.None);
            loader.AllowLoad.TrySetResult(true);
            MapLoadResult r1 = await first;
            MapLoadResult r2 = await second;
            Assert.IsFalse(r1.Success);
            Assert.AreEqual(MapLoadFailureReason.Cancelled, r1.FailureReason);
            Assert.IsTrue(r2.Success);
            Assert.AreEqual(MapId.Map02, s.Runtime.CurrentMapId);
        }

        [Test] public async Task TryGetContext_OnlyWhenLoaded()
        {
            var s = Setup();
            Assert.IsFalse(s.Runtime.TryGetCurrentContext(out _));
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            Assert.IsTrue(s.Runtime.TryGetCurrentContext(out var context));
            Assert.AreEqual(MapId.Map01, context.MapId);
        }

        [Test] public async Task Shutdown_UnloadsAndDeinitializes()
        {
            var s = Setup();
            await s.Runtime.LoadAsync(MapId.Map01, CancellationToken.None);
            await s.Configurator.ShutdownAsync(CancellationToken.None);
            Assert.IsFalse(s.Runtime.IsInitialized);
            Assert.AreEqual(1, s.Loader.ReleaseCount);
        }

        private static readonly MapBounds ValidBounds = new MapBounds(-10, 10, -10, 10);
        private static readonly MapPoint Zero = new MapPoint(0, 0, 0);
        private static readonly MapPoint SoldierSpawn = new MapPoint(2, 0, -3);

        private static MapDefinition Def(MapId id) => new MapDefinition(id, id.ToString());
        private static MapSpawnSector Sector(MapSpawnSectorId id)
        {
            var area = new MapArea(-1, 1, -1, 1);
            return new MapSpawnSector(id, in area);
        }
        private static MapSpawnSector[] FourSectors() => new[] { Sector(MapSpawnSectorId.Top), Sector(MapSpawnSectorId.Bottom), Sector(MapSpawnSectorId.Left), Sector(MapSpawnSectorId.Right) };
        private static MapRuntimeContext Context(MapId id) => new MapRuntimeContext(id, in ValidBounds, in ValidBounds, FourSectors(), in SoldierSpawn, in Zero, true);
        private static IMapCatalog Catalog() => new MapCatalog(new[] { Def(MapId.Map01), Def(MapId.Map02) });

        private static SetupResult Setup(IMapCatalog catalog = null, FakeLoader loader = null)
        {
            var bus = new EventBus();
            loader ??= new FakeLoader();
            var runtime = new MapRuntime(bus);
            runtime.Initialize(catalog ?? Catalog(), loader);
            return new SetupResult(bus, runtime, runtime, loader);
        }

        private readonly struct SetupResult
        {
            public EventBus Bus { get; }
            public MapRuntime Runtime { get; }
            public IMapRuntimeConfigurator Configurator { get; }
            public FakeLoader Loader { get; }
            public SetupResult(EventBus bus, MapRuntime runtime, IMapRuntimeConfigurator configurator, FakeLoader loader)
            {
                Bus = bus; Runtime = runtime; Configurator = configurator; Loader = loader;
            }
        }

        private sealed class FakeView : IMapView
        {
            private readonly bool _valid;
            public MapId Id { get; }
            public FakeView(MapId id, bool valid = true) { Id = id; _valid = valid; }
            public bool TryBuildContext(out MapRuntimeContext context, out string error)
            {
                if (!_valid) { context = null; error = "invalid"; return false; }
                context = Context(Id); error = string.Empty; return true;
            }
        }

        private sealed class FakeInstance : IMapInstance
        {
            public MapId MapId { get; }
            public IMapView View { get; }
            public FakeInstance(MapId id, IMapView view) { MapId = id; View = view; }
        }

        private sealed class FakeLoader : IMapAssetLoader
        {
            public int LoadCount { get; private set; }
            public int ReleaseCount { get; private set; }
            public bool ThrowOnLoad { get; set; }
            public bool ReturnNull { get; set; }
            public bool MissingView { get; set; }
            public bool InvalidContext { get; set; }
            public MapId ForceInstanceId { get; set; } = MapId.None;
            public bool DelayLoad { get; set; }
            public bool DelayFirstLoadOnly { get; set; }
            public TaskCompletionSource<bool> LoadEntered { get; } = new TaskCompletionSource<bool>();
            public TaskCompletionSource<bool> AllowLoad { get; } = new TaskCompletionSource<bool>();

            public async Task<IMapInstance> LoadAsync(MapDefinition definition, CancellationToken cancellationToken)
            {
                LoadCount++;
                if (ThrowOnLoad) throw new InvalidOperationException("loader failure");
                bool shouldDelay = DelayLoad || (DelayFirstLoadOnly && LoadCount == 1);
                if (shouldDelay)
                {
                    LoadEntered.TrySetResult(true);
                    await AllowLoad.Task;
                    cancellationToken.ThrowIfCancellationRequested();
                }
                if (ReturnNull) return null;
                MapId instanceId = ForceInstanceId == MapId.None ? definition.Id : ForceInstanceId;
                IMapView view = MissingView ? null : new FakeView(instanceId, !InvalidContext);
                return new FakeInstance(instanceId, view);
            }

            public Task ReleaseAsync(IMapInstance instance, CancellationToken cancellationToken)
            {
                ReleaseCount++;
                return Task.CompletedTask;
            }
        }
    }
}
