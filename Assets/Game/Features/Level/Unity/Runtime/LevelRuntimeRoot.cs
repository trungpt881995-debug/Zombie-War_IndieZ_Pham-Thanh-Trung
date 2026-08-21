using System;
using UnityEngine;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Services;
using ZombieWar.Features.Level.Unity.Config;
namespace ZombieWar.Features.Level.Unity.Runtime
{
    public sealed class LevelRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private LevelCatalogConfig catalogConfig;
        private ILevelRuntime _runtime;
        private ILevelRuntimeConfigurator _configurator;
        public bool IsInitialized => _runtime != null && _runtime.IsInitialized;
        public ILevelRuntime Runtime => _runtime;
        public void Initialize(ILevelRuntime runtime, ILevelRuntimeConfigurator configurator)
        {
            if (IsInitialized)
                return;

            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            if (catalogConfig == null)
                throw new InvalidOperationException("LevelCatalogConfig is not assigned.");

            configurator.Initialize(catalogConfig.BuildCatalog());
            _runtime = runtime;
            _configurator = configurator;
        }
        public bool BeginGameLevel(GameLevelId id) => _runtime != null && _runtime.BeginLevel(id);
        public bool AddNormalZombieKill() => _runtime != null && _runtime.RegisterNormalZombieKill();
        public bool AddNormalZombieKills(int count) => _runtime != null && _runtime.RegisterNormalZombieKills(count);
        public bool RegisterBossDefeated(LevelBossObjectiveId boss) => _runtime != null && _runtime.RegisterBossDefeated(boss);
        public void SetProgressionEnabled(bool enabled) => _runtime?.SetProgressionEnabled(enabled);
        private void OnDestroy()
        {
            _configurator?.Shutdown();
            _runtime = null;
            _configurator = null;
        }
    }
}
