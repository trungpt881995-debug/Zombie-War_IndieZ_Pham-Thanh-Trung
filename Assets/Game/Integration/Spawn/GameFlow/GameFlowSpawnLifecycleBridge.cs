using System;
using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Spawn.Commands;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.Integration.GameState.GameFlow
{
    /// <summary>
    /// Starts normal Zombie spawning exactly once after a Loading -> Gameplay transition.
    ///
    /// Stop ownership remains in GameStateGameplayGateBridge:
    /// Inactive/terminal gameplay states stop Spawn with the appropriate reason.
    /// This bridge only owns the GameFlow-side start intent after loading is complete.
    ///
    /// The Loading arm prevents Pause -> Gameplay resume from restarting Spawn after
    /// BossPhase or another terminal Spawn stop reason.
    /// </summary>
    public sealed class GameFlowSpawnLifecycleBridge : IStartable, IDisposable
    {
        private readonly GameFlowModel _flowModel;
        private readonly ICommandBus _commands;

        private bool _startAfterLoading;
        private bool _started;

        public GameFlowSpawnLifecycleBridge(
            GameFlowModel flowModel,
            ICommandBus commands)
        {
            _flowModel = flowModel ?? throw new ArgumentNullException(nameof(flowModel));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _flowModel.StateChanged += OnGameFlowStateChanged;
            ApplyCurrentState(_flowModel.CurrentState);
        }

        public void Dispose()
        {
            if (!_started)
            {
                return;
            }

            _started = false;
            _flowModel.StateChanged -= OnGameFlowStateChanged;
            _startAfterLoading = false;
        }

        private void OnGameFlowStateChanged(GameFlowStateId state)
        {
            ApplyCurrentState(state);
        }

        private void ApplyCurrentState(GameFlowStateId state)
        {
            switch (state)
            {
                case GameFlowStateId.Loading:
                    _startAfterLoading = true;
                    break;

                case GameFlowStateId.Gameplay:
                    if (!_startAfterLoading)
                    {
                        return;
                    }

                    _startAfterLoading = false;
                    _commands.Send(new StartZombieSpawningCommand());
                    break;

                case GameFlowStateId.Boot:
                case GameFlowStateId.MainMenu:
                    _startAfterLoading = false;
                    break;
            }
        }
    }
}
