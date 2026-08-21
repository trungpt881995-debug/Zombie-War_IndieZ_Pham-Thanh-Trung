using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Level.Catalog;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Events;
using ZombieWar.Features.Level.Model;

namespace ZombieWar.Features.Level.Controller
{
    public sealed class LevelController : IController
    {
        private readonly LevelModel _model;
        private readonly ILevelCatalog _catalog;
        private readonly IEventBus _events;
        private LevelDefinition _definition;
        private int _nextProgressionIndex;
        public LevelController(LevelModel model, ILevelCatalog catalog, IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _model.SetReady();
        }
        public bool BeginLevel(GameLevelId id)
        {
            if (!_catalog.TryGet(id, out LevelDefinition d)) return false;
            _definition = d;
            _model.Begin(d);
            _nextProgressionIndex = 1;
            _events.Publish(new GameLevelStartedEvent(_model.GameLevel, _model.SoldierGroupLevel));
            PublishProgress();
            return true;
        }
        public bool RegisterNormalZombieKill() => RegisterNormalZombieKills(1);
        public bool RegisterNormalZombieKills(int count)
        {
            if (count <= 0 || _definition == null || _model.State != LevelState.Running || !_model.ProgressionEnabled) return false;
            _model.AddKills(count);
            PublishProgress();
            if (_model.Phase == LevelPhase.NormalCombat)
            {
                EvaluateSoldierProgression();
                EvaluateBossPhase();
            }
            return true;
        }
        public bool RegisterBossDefeated(LevelBossObjectiveId boss)
        {
            if (_definition == null || _model.State != LevelState.Running || _model.Phase != LevelPhase.BossPhase) return false;
            if (boss == LevelBossObjectiveId.None || (((int) boss&((int) boss - 1)) != 0)) return false;
            if ((_model.RequiredBossObjectives& boss) == 0) return false;
            if ((_model.DefeatedBossObjectives& boss) != 0) return false;
            _model.AddDefeatedBoss(boss);
            _events.Publish(new BossObjectiveCompletedEvent(_model.GameLevel, boss));
            if (_model.BossObjectivesComplete) CompleteCurrentLevel();
            return true;
        }
        public void SetProgressionEnabled(bool enabled)
        {
            _model.SetProgressionEnabled(enabled);
        }
        public LevelProgressSnapshot Snapshot()
        {
            int next = 0;
            if (_definition != null && _model.State == LevelState.Running) next = _model.Phase == LevelPhase.NormalCombat ? _definition.GetNextThreshold(_model.SoldierGroupLevel) : _definition.BossPhaseKillThreshold;
            return new LevelProgressSnapshot(_model.GameLevel, _model.SoldierGroupLevel, _model.NormalZombieKillCount, next,
            _model.State, _model.Phase, _model.ProgressionEnabled, _model.RequiredBossObjectives, _model.DefeatedBossObjectives);
        }
        public void Shutdown()
        {
            _definition = null;
            _nextProgressionIndex = 0;
            _model.Reset();
        }
        private void EvaluateSoldierProgression()
        {
            while (_nextProgressionIndex < _definition.ProgressionStepCount)
            {
                SoldierProgressionStep step = _definition.GetProgressionStep(_nextProgressionIndex);
                if (_model.NormalZombieKillCount < step.RequiredTotalKills) break;
                var previous = _model.SoldierGroupLevel;
                _model.SetSoldierGroupLevel(step.Level);
                _nextProgressionIndex++;
                _events.Publish(new SoldierGroupLevelChangedEvent(_model.GameLevel, previous, _model.SoldierGroupLevel, _model.NormalZombieKillCount));
            }
        }
        private void EvaluateBossPhase()
        {
            if (_model.Phase != LevelPhase.NormalCombat || _model.NormalZombieKillCount < _definition.BossPhaseKillThreshold) return;
            _model.StartBossPhase();
            _events.Publish(new BossPhaseStartedEvent(_model.GameLevel, _model.NormalZombieKillCount, _model.RequiredBossObjectives));
        }
        private void CompleteCurrentLevel()
        {
            if (_model.State == LevelState.Completed) return;
            _model.Complete();
            _events.Publish(new GameLevelCompletedEvent(_model.GameLevel, _definition.IsFinalLevel));
            if (_definition.IsFinalLevel) _events.Publish(new GameCompletedEvent(_model.GameLevel));
        }
        private void PublishProgress()
        {
            _events.Publish(new LevelKillProgressChangedEvent(_model.GameLevel, _model.SoldierGroupLevel, _model.NormalZombieKillCount,
            _definition.GetNextThreshold(_model.SoldierGroupLevel)));
        }
    }
}
