using System;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Services;
using ZombieWar.Features.Score.Unity.Config;

namespace ZombieWar.Features.Score.Unity.Runtime
{
    public sealed class ScoreRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private ScoreConfig config;
        [SerializeField] private bool startRunOnInitialize;

        private IScoreRuntime _runtime;
        private IScoreRuntimeConfigurator _configurator;

        public bool IsInitialized { get; private set; }
        public IScoreRuntime Runtime => _runtime;

        public void Initialize(IScoreRuntime runtime, IScoreRuntimeConfigurator configurator)
        {
            if (IsInitialized) return;
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            if (config == null) throw new InvalidOperationException("ScoreRuntimeRoot requires ScoreConfig.");

            var catalog = config.BuildCatalog();
            configurator.Initialize(catalog);
            _runtime = runtime;
            _configurator = configurator;
            IsInitialized = true;

            if (startRunOnInitialize)
                _runtime.StartRun();
        }

        public void StartRun() => _runtime?.StartRun();
        public bool BeginLevel(int gameLevel) => _runtime != null && _runtime.BeginLevel((ScoreLevelId)gameLevel);
        public bool ReplayCurrentLevel() => _runtime != null && _runtime.ReplayCurrentLevel();
        public void SetScoringEnabled(bool enabled) => _runtime?.SetScoringEnabled(enabled);
        public ScoreAwardResult Award(ScoreActionId actionId, long sourceEntityId) =>
            _runtime != null
                ? _runtime.Award(actionId, new GameplayEntityId(sourceEntityId))
                : ScoreAwardResult.Rejected(0, 0);

        private void OnDestroy()
        {
            if (!IsInitialized) return;
            _configurator?.Shutdown();
            _runtime = null;
            _configurator = null;
            IsInitialized = false;
        }
    }
}
