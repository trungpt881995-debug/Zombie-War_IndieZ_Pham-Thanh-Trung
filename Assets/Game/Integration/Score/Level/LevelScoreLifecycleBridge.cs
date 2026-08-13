using System;
using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Level.Domain;
using ZombieWar.Features.Level.Events;
using ZombieWar.Features.Score.Commands;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Services;

namespace ZombieWar.Integration.Score.Level
{
    public sealed class LevelScoreLifecycleBridge : IStartable, IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly ICommandBus _commands;
        private readonly IScoreRuntime _score;
        private IDisposable _subscription;

        public LevelScoreLifecycleBridge(IEventSubscriber events, ICommandBus commands, IScoreRuntime score)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _score = score ?? throw new ArgumentNullException(nameof(score));
        }

        public void Start()
        {
            _subscription?.Dispose();
            _subscription = _events.Subscribe<GameLevelStartedEvent>(OnLevelStarted);
        }

        private void OnLevelStarted(GameLevelStartedEvent evt)
        {
            ScoreLevelId mapped = evt.GameLevel == GameLevelId.GameLevel01 ? ScoreLevelId.GameLevel01
                : evt.GameLevel == GameLevelId.GameLevel02 ? ScoreLevelId.GameLevel02
                : ScoreLevelId.None;
            if (mapped == ScoreLevelId.None) return;

            // Same Game Level beginning again is treated as replay. New Game must explicitly
            // send StartScoreRunCommand before BeginGameLevelCommand.
            if (_score.State == ScoreState.Running && _score.CurrentLevel == mapped)
                _commands.Send(new ReplayScoreLevelCommand());
            else
                _commands.Send(new BeginScoreLevelCommand(mapped));
        }

        public void Dispose() { _subscription?.Dispose(); _subscription = null; }
    }
}
