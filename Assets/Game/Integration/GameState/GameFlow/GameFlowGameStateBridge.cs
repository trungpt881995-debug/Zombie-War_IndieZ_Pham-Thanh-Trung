using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Commands;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Services;
using ZombieWar.GameFlow.Controller;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.Integration.GameState.GameFlow
{
    /// <summary>
    /// Guarded bidirectional bridge for the CURRENT GameFlow implementation.
    /// GameFlow owns Boot/MainMenu/Loading and gameplay lifecycle entry/exit.
    /// GameState owns runtime Pause/GameOver/LevelComplete/EndGame.
    /// Guards prevent echo loops while still supporting legacy GameFlow commands.
    /// </summary>
    public sealed class GameFlowGameStateBridge : IDisposable
    {
        private readonly GameFlowModel _flowModel;
        private readonly GameFlowController _flowController;
        private readonly IGameStateRuntime _gameState;
        private readonly ICommandBus _commands;
        private readonly IEventSubscriber _events;
        private readonly ZombieWar.Integration.GameState.Runtime.GameStateGameplayGateBridge _gates;
        private IDisposable _stateSubscription;
        private bool _started;

        public GameFlowGameStateBridge(
            GameFlowModel flowModel,
            GameFlowController flowController,
            IGameStateRuntime gameState,
            ICommandBus commands,
            IEventSubscriber events,
            ZombieWar.Integration.GameState.Runtime.GameStateGameplayGateBridge gates)
        {
            _flowModel = flowModel ?? throw new ArgumentNullException(nameof(flowModel));
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _gates = gates ?? throw new ArgumentNullException(nameof(gates));
        }

        public void Start()
        {
            if (_started) return;
            _started = true;
            _flowModel.StateChanged += OnGameFlowStateChanged;
            _stateSubscription = _events.Subscribe<GameplayStateChangedEvent>(OnGameplayStateChanged);
            OnGameFlowStateChanged(_flowModel.CurrentState);
        }

        public void Dispose()
        {
            if (!_started) return;
            _started = false;
            _flowModel.StateChanged -= OnGameFlowStateChanged;
            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void OnGameFlowStateChanged(GameFlowStateId flow)
        {
            GameplayStateId state = _gameState.State;
            switch (flow)
            {
                case GameFlowStateId.Boot:
                case GameFlowStateId.MainMenu:
                case GameFlowStateId.Loading:
                    if (state != GameplayStateId.Inactive)
                        _commands.Send(new DeactivateGameplayCommand());
                    break;

                case GameFlowStateId.Gameplay:
                    if (state == GameplayStateId.Inactive)
                        _commands.Send(new BeginGameplayCommand());
                    else if (state == GameplayStateId.Paused)
                        _commands.Send(new ResumeGameplayCommand());
                    break;

                case GameFlowStateId.Paused:
                    if (state == GameplayStateId.Playing)
                        _commands.Send(new PauseGameplayCommand());
                    break;

                case GameFlowStateId.GameOver:
                    if (state == GameplayStateId.Playing || state == GameplayStateId.Paused)
                        _commands.Send(new EnterGameOverCommand());
                    break;

                case GameFlowStateId.LevelComplete:
                    if (state == GameplayStateId.Playing || state == GameplayStateId.Paused)
                        _commands.Send(new EnterLevelCompleteCommand());
                    break;

                case GameFlowStateId.EndGame:
                    if (state == GameplayStateId.Playing || state == GameplayStateId.Paused)
                        _commands.Send(new EnterEndGameCommand());
                    break;
            }

            // Re-apply even when GameState did not change. This keeps clock/input/runtime gates
            // correct regardless of VContainer entry-point ordering and legacy GameFlow calls.
            _gates.ReapplyCurrentState();
        }

        private void OnGameplayStateChanged(GameplayStateChangedEvent evt)
        {
            switch (evt.Current)
            {
                case GameplayStateId.Paused:
                    if (_flowModel.CurrentState != GameFlowStateId.Paused)
                        _flowController.Pause();
                    break;

                case GameplayStateId.Playing:
                    if (evt.Previous == GameplayStateId.Paused)
                        _flowController.Resume();
                    // Inactive -> Playing is initiated by GameFlow; do not echo BeginGameplay.
                    break;

                case GameplayStateId.GameOver:
                    if (_flowModel.CurrentState != GameFlowStateId.GameOver)
                        _flowController.GameOver();
                    break;

                case GameplayStateId.LevelComplete:
                    if (_flowModel.CurrentState != GameFlowStateId.LevelComplete)
                        _flowController.LevelComplete();
                    break;

                case GameplayStateId.EndGame:
                    if (_flowModel.CurrentState != GameFlowStateId.EndGame)
                        _flowController.EndGame();
                    break;
            }
        }
    }
}
