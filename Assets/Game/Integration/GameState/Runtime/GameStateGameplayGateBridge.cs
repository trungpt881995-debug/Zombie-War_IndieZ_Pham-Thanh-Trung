using System;
using GeneralCore.Architecture;
using GeneralCore.UIInput;
using GameplayCore.Time;
using ZombieWar.Features.Boss.Services;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Services;
using ZombieWar.Features.Level.Services;
using ZombieWar.Features.Score.Services;
using ZombieWar.Features.Spawn.Domain;
using ZombieWar.Features.Spawn.Services;

namespace ZombieWar.Integration.GameState.Runtime
{
    public sealed class GameStateGameplayGateBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IGameStateRuntime _gameState;
        private readonly IGameplayClockControl _clock;
        private readonly IInputGate _input;
        private readonly IGameStateSoldierGate _soldier;
        private readonly IGameStateSceneGateRegistry _sceneGates;
        private readonly ISpawnRuntime _spawn;
        private readonly IBossRuntime _boss;
        private readonly ILevelRuntime _level;
        private readonly IScoreRuntime _score;
        private IDisposable _subscription;

        public GameStateGameplayGateBridge(
            IEventSubscriber events,
            IGameStateRuntime gameState,
            IGameplayClockControl clock,
            IInputGate input,
            IGameStateSoldierGate soldier,
            IGameStateSceneGateRegistry sceneGates,
            ISpawnRuntime spawn,
            IBossRuntime boss,
            ILevelRuntime level,
            IScoreRuntime score)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _soldier = soldier ?? throw new ArgumentNullException(nameof(soldier));
            _sceneGates = sceneGates ?? throw new ArgumentNullException(nameof(sceneGates));
            _spawn = spawn ?? throw new ArgumentNullException(nameof(spawn));
            _boss = boss ?? throw new ArgumentNullException(nameof(boss));
            _level = level ?? throw new ArgumentNullException(nameof(level));
            _score = score ?? throw new ArgumentNullException(nameof(score));
        }

        public void Start()
        {
            if (_subscription != null) return;
            _subscription = _events.Subscribe<GameplayStateChangedEvent>(OnStateChanged);
            Apply(_gameState.State);
        }

        public void ReapplyCurrentState() => Apply(_gameState.State);

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnStateChanged(GameplayStateChangedEvent evt) => Apply(evt.Current);

        private void Apply(GameplayStateId state)
        {
            bool playing = state == GameplayStateId.Playing;
            _clock.SetPaused(!playing);
            _input.SetGameplayInputEnabled(playing);
            _soldier.SetGameplayEnabled(playing);
            _sceneGates.SetGameplayEnabled(playing);
            _boss.SetGameplayEnabled(playing);
            _level.SetProgressionEnabled(playing);

            // Preserve completion-transaction scoring regardless of synchronous subscriber order.
            bool scoringEnabled = playing || state == GameplayStateId.LevelComplete || state == GameplayStateId.EndGame;
            _score.SetScoringEnabled(scoringEnabled);

            switch (state)
            {
                case GameplayStateId.Playing:
                    _spawn.SetGameplayEnabled(true);
                    break;
                case GameplayStateId.Paused:
                    _spawn.SetGameplayEnabled(false);
                    break;
                case GameplayStateId.GameOver:
                    _spawn.Stop(SpawnStopReason.GameOver);
                    break;
                case GameplayStateId.LevelComplete:
                case GameplayStateId.EndGame:
                    _spawn.Stop(SpawnStopReason.LevelComplete);
                    break;
                case GameplayStateId.Inactive:
                    _spawn.Stop(SpawnStopReason.LevelTransition);
                    break;
            }
        }
    }
}
