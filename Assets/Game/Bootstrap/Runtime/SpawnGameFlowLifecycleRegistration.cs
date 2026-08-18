using System;
using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Spawn.Commands;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;

namespace ZombieWar.Bootstrap
{
    
    // Starts Zombie spawning only after a Loading -> Gameplay transition.
    // Pause -> Gameplay does not restart Spawn, preserving explicit stops such as BossPhase.
    
    public sealed class SpawnGameFlowLifecycleRegistration : IStartable, IDisposable
    {
        private readonly GameFlowModel _flowModel;
        private readonly ICommandBus _commands;

        private bool _startAfterLoading;
        private bool _started;

        public SpawnGameFlowLifecycleRegistration(GameFlowModel flowModel, ICommandBus commands)
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
