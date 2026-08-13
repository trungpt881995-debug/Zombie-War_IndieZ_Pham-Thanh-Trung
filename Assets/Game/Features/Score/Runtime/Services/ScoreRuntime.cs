using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Score.Catalog;
using ZombieWar.Features.Score.Controller;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Model;

namespace ZombieWar.Features.Score.Services
{
    public sealed class ScoreRuntime : IScoreRuntime, IScoreRuntimeConfigurator
    {
        private readonly IEventBus _events;
        private ScoreModel _model;
        private ScoreController _controller;

        public ScoreRuntime(IEventBus events) => _events = events ?? throw new ArgumentNullException(nameof(events));

        public bool IsInitialized => _controller != null;
        public ScoreState State => _model != null ? _model.State : ScoreState.Uninitialized;
        public bool ScoringEnabled => _model != null && _model.ScoringEnabled;
        public long TotalScore => _model != null ? _model.TotalScore : 0;
        public long LevelScore => _model != null ? _model.LevelScore : 0;
        public ScoreLevelId CurrentLevel => _model != null ? _model.CurrentLevel : ScoreLevelId.None;
        public ScoreSnapshot Snapshot => _model != null
            ? _model.Snapshot()
            : new ScoreSnapshot(ScoreState.Uninitialized, false, 0, 0, 0, ScoreLevelId.None);

        public void Initialize(IScoreRuleCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (IsInitialized) throw new InvalidOperationException("ScoreRuntime is already initialized.");
            _model = new ScoreModel();
            _controller = new ScoreController(_model, catalog, _events);
            _controller.Initialize();
        }

        public void Shutdown()
        {
            _controller = null;
            _model = null;
        }

        public void StartRun()
        {
            if (!IsInitialized) return;
            _controller.StartRun();
        }

        public bool BeginLevel(ScoreLevelId level) => IsInitialized && _controller.BeginLevel(level);
        public bool ReplayCurrentLevel() => IsInitialized && _controller.ReplayCurrentLevel();

        public ScoreAwardResult Award(ScoreActionId actionId, EntityId sourceEntityId)
        {
            if (!IsInitialized)
                return ScoreAwardResult.Rejected(0, 0);

            var context = new ScoreContext(actionId, sourceEntityId, CurrentLevel);
            return _controller.Award(in context);
        }

        public void SetScoringEnabled(bool enabled)
        {
            if (IsInitialized) _controller.SetScoringEnabled(enabled);
        }
    }
}
