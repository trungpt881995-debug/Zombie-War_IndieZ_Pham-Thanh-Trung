using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Commands;
using ZombieWar.Features.Level.Events;

namespace ZombieWar.Integration.GameState.Level
{
    public sealed class LevelGameStateBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly ICommandBus _commands;
        private IDisposable _levelCompleted;
        private IDisposable _gameCompleted;

        public LevelGameStateBridge(IEventSubscriber events, ICommandBus commands)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void Start()
        {
            if (_levelCompleted != null) return;
            _levelCompleted = _events.Subscribe<GameLevelCompletedEvent>(OnLevelCompleted);
            _gameCompleted = _events.Subscribe<GameCompletedEvent>(OnGameCompleted);
        }

        public void Dispose()
        {
            _levelCompleted?.Dispose();
            _gameCompleted?.Dispose();
            _levelCompleted = null;
            _gameCompleted = null;
        }

        private void OnLevelCompleted(GameLevelCompletedEvent evt)
        {
            // Final Game Level must go directly to EndGame via GameCompletedEvent.
            if (evt.IsFinalLevel) return;
            _commands.Send(new EnterLevelCompleteCommand());
        }

        private void OnGameCompleted(GameCompletedEvent evt) =>
            _commands.Send(new EnterEndGameCommand());
    }
}
