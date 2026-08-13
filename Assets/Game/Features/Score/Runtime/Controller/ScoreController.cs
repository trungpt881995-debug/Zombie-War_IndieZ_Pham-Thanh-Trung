using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Score.Catalog;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Events;
using ZombieWar.Features.Score.Model;
using ZombieWar.Features.Score.Rules;

namespace ZombieWar.Features.Score.Controller
{
    public sealed class ScoreController
    {
        private readonly ScoreModel _model;
        private readonly IScoreRuleCatalog _catalog;
        private readonly IEventBus _events;

        public ScoreController(ScoreModel model, IScoreRuleCatalog catalog, IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void Initialize() => _model.Initialize();

        public void StartRun()
        {
            _model.StartRun();
            ScoreSnapshot snapshot = _model.Snapshot();
            _events.Publish(new ScoreRunStartedEvent(in snapshot));
        }

        public bool BeginLevel(ScoreLevelId level)
        {
            if (_model.State != ScoreState.Running ||
                (level != ScoreLevelId.GameLevel01 && level != ScoreLevelId.GameLevel02))
                return false;

            _model.BeginLevel(level);
            ScoreSnapshot snapshot = _model.Snapshot();
            _events.Publish(new ScoreLevelStartedEvent(in snapshot));
            return true;
        }

        public bool ReplayCurrentLevel()
        {
            if (_model.State != ScoreState.Running || _model.CurrentLevel == ScoreLevelId.None)
                return false;

            long previous = _model.TotalScore;
            _model.ReplayLevel();
            ScoreSnapshot snapshot = _model.Snapshot();
            _events.Publish(new ScoreLevelReplayedEvent(previous, in snapshot));
            return true;
        }

        public ScoreAwardResult Award(in ScoreContext context)
        {
            if (_model.State != ScoreState.Running ||
                !_model.ScoringEnabled ||
                _model.CurrentLevel == ScoreLevelId.None ||
                context.Level != _model.CurrentLevel ||
                context.ActionId == ScoreActionId.None ||
                context.SourceEntityId.Value <= 0)
            {
                return ScoreAwardResult.Rejected(_model.TotalScore, _model.LevelScore);
            }

            if (!_catalog.TryGet(context.ActionId, out IScoreRule rule))
                return ScoreAwardResult.Rejected(_model.TotalScore, _model.LevelScore);

            long amount = rule.Calculate(in context);
            if (amount <= 0)
                return ScoreAwardResult.Rejected(_model.TotalScore, _model.LevelScore);

            long newTotal;
            long newLevel;
            try
            {
                checked
                {
                    newTotal = _model.TotalScore + amount;
                    newLevel = _model.LevelScore + amount;
                }
            }
            catch (OverflowException)
            {
                return ScoreAwardResult.Rejected(_model.TotalScore, _model.LevelScore);
            }

            long previous = _model.TotalScore;
            _model.CommitAward(newTotal, newLevel);
            _events.Publish(new ScoreChangedEvent(
                previous,
                newTotal,
                amount,
                newLevel,
                context.ActionId,
                context.SourceEntityId));

            return new ScoreAwardResult(true, amount, newTotal, newLevel);
        }

        public void SetScoringEnabled(bool enabled)
        {
            if (_model.State != ScoreState.Running || _model.ScoringEnabled == enabled)
                return;

            _model.SetScoringEnabled(enabled);
            _events.Publish(new ScoringEnabledChangedEvent(enabled));
        }

        public ScoreSnapshot Snapshot() => _model.Snapshot();
    }
}
