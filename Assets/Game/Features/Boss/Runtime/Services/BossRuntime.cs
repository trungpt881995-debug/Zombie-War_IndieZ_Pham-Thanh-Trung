using System;
using ZombieWar.Features.Boss.Catalog;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Features.Boss.Registry;

namespace ZombieWar.Features.Boss.Services
{
    public sealed class BossRuntime : IBossRuntime, IBossRuntimeConfigurator
    {
        private IBossCatalog _catalog;
        private IBossSpawnExecutor _executor;
        private IActiveBossRegistry _registry;
        public bool IsInitialized
        {
            get;
            private set;
        }
        public int ActiveCount => _registry != null ? _registry.Count : 0;
        public void Initialize(IBossCatalog catalog, IBossSpawnExecutor spawnExecutor, IActiveBossRegistry registry)
        {
            if (IsInitialized) return;
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _executor = spawnExecutor ?? throw new ArgumentNullException(nameof(spawnExecutor));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            IsInitialized = true;
        }
        public bool TrySpawn(in BossSpawnSelection selection, in BossPoint anchor)
        {
            if (!IsInitialized || selection.Count < 1 || selection.Count > 2) return false;
            BossId firstId = selection.First;
            if (!_catalog.TryGet(firstId, out BossDefinition d0)) return false;
            BossPoint p0 = anchor.Add(d0.SpawnOffsetX, d0.SpawnOffsetY, d0.SpawnOffsetZ);
            var r0 = new BossSpawnRequest(firstId, in p0);
            if (selection.Count == 1)
            {
                var plan1 = new BossSpawnPlan(in r0);
                return _executor.TrySpawnPlan(in plan1);
            }
            BossId secondId = selection.Second;
            if (!_catalog.TryGet(secondId, out BossDefinition d1)) return false;
            BossPoint p1 = anchor.Add(d1.SpawnOffsetX, d1.SpawnOffsetY, d1.SpawnOffsetZ);
            var r1 = new BossSpawnRequest(secondId, in p1);
            var plan2 = new BossSpawnPlan(in r0, in r1);
            return _executor.TrySpawnPlan(in plan2);
        }
        public void SetGameplayEnabled(bool enabled)
        {
            if (_registry == null) return;
            var active = _registry.Active;
            for (int i = active.Count - 1; i >= 0; i--) active[i].SetGameplayEnabled(enabled);
        }
        public void CancelAll()
        {
            if (_registry == null) return;
            for (int i = _registry.Active.Count - 1; i >= 0; i--) _registry.Active[i].Cancel();
        }
        public void Shutdown()
        {
            CancelAll();
            _catalog = null;
            _executor = null;
            _registry = null;
            IsInitialized = false;
        }
    }
}
