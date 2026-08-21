using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Level.Catalog;
using ZombieWar.Features.Level.Controller;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Model;
namespace ZombieWar.Features.Level.Services
{
    public sealed class LevelRuntime : ILevelRuntime, ILevelRuntimeConfigurator
    {
        private readonly IEventBus _events;
        private LevelModel _model;
        private LevelController _controller;
        public bool IsInitialized => _controller != null;
        public LevelState State => IsInitialized ? _model.State : LevelState.Uninitialized;
        public LevelPhase Phase => IsInitialized ? _model.Phase : LevelPhase.None;
        public GameLevelId GameLevel => IsInitialized ? _model.GameLevel : GameLevelId.None;
        public SoldierGroupLevelId SoldierGroupLevel => IsInitialized ? _model.SoldierGroupLevel : SoldierGroupLevelId.Level1;
        public int NormalZombieKillCount => IsInitialized ? _model.NormalZombieKillCount : 0;
        public bool ProgressionEnabled => IsInitialized && _model.ProgressionEnabled;
        public LevelRuntime(IEventBus events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }
        public void Initialize(ILevelCatalog catalog)
        {
            if (IsInitialized)
                throw new InvalidOperationException("LevelRuntime is already initialized.");

            _model = new LevelModel();
            _controller = new LevelController(_model, catalog, _events);
        }
        public bool BeginLevel(GameLevelId id) => IsInitialized && _controller.BeginLevel(id);
        public bool RegisterNormalZombieKill() => IsInitialized && _controller.RegisterNormalZombieKill();
        public bool RegisterNormalZombieKills(int count) => IsInitialized && _controller.RegisterNormalZombieKills(count);
        public bool RegisterBossDefeated(LevelBossObjectiveId boss) => IsInitialized && _controller.RegisterBossDefeated(boss);
        public void SetProgressionEnabled(bool enabled)
        {
            if (IsInitialized)
                _controller.SetProgressionEnabled(enabled);
        }
        public LevelProgressSnapshot Snapshot() => IsInitialized ? _controller.Snapshot() : default;
        public void Shutdown()
        {
            if (!IsInitialized)
                return;

            _controller.Shutdown();
            _controller = null;
            _model = null;
        }
    }
}
