using System;
using ZombieWar.Features.GameState.Controller;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Model;

namespace ZombieWar.Features.GameState.Services
{
    public sealed class GameStateRuntime : IGameStateRuntime, IGameStateRuntimeConfigurator
    {
        private readonly GameStateModel _model;
        private readonly GameStateController _controller;

        public bool IsInitialized { get; private set; }
        public GameplayStateId State => _model.Current;
        public GameplayStateSnapshot Snapshot => _model.Snapshot();

        public GameStateRuntime(GameStateModel model, GameStateController controller)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Initialize()
        {
            if (IsInitialized) return;
            _model.Reset();
            IsInitialized = true;
        }

        public void Shutdown()
        {
            _model.Reset();
            IsInitialized = false;
        }

        public GameplayStateTransitionResult BeginGameplay() =>
            TransitionFrom(
                GameplayStateId.Inactive,
                GameplayStateId.Playing,
                GameplayStateTransitionReason.GameFlowGameplayReady);

        public GameplayStateTransitionResult Pause() =>
            TransitionFrom(
                GameplayStateId.Playing,
                GameplayStateId.Paused,
                GameplayStateTransitionReason.UserPauseRequested);

        public GameplayStateTransitionResult Resume() =>
            TransitionFrom(
                GameplayStateId.Paused,
                GameplayStateId.Playing,
                GameplayStateTransitionReason.UserResumeRequested);

        public GameplayStateTransitionResult EnterGameOver() =>
            Transition(GameplayStateId.GameOver, GameplayStateTransitionReason.SoldierGroupDefeated);

        public GameplayStateTransitionResult EnterLevelComplete() =>
            Transition(GameplayStateId.LevelComplete, GameplayStateTransitionReason.GameLevelCompleted);

        public GameplayStateTransitionResult EnterEndGame() =>
            Transition(GameplayStateId.EndGame, GameplayStateTransitionReason.GameCompleted);

        public GameplayStateTransitionResult Deactivate() =>
            Transition(GameplayStateId.Inactive, GameplayStateTransitionReason.GameFlowDeactivated);

        private GameplayStateTransitionResult TransitionFrom(
            GameplayStateId requiredCurrent,
            GameplayStateId target,
            GameplayStateTransitionReason reason)
        {
            if (!IsInitialized)
                return GameplayStateTransitionResult.Rejected(
                    _model.Current,
                    reason,
                    GameplayStateTransitionFailure.NotInitialized);

            if (_model.Current == target)
                return GameplayStateTransitionResult.Rejected(
                    _model.Current,
                    reason,
                    GameplayStateTransitionFailure.SameState);

            if (_model.Current != requiredCurrent)
                return GameplayStateTransitionResult.Rejected(
                    _model.Current,
                    reason,
                    GameplayStateTransitionFailure.InvalidTransition);

            return _controller.TryTransition(target, reason);
        }

        private GameplayStateTransitionResult Transition(
            GameplayStateId target,
            GameplayStateTransitionReason reason)
        {
            if (!IsInitialized)
                return GameplayStateTransitionResult.Rejected(
                    _model.Current,
                    reason,
                    GameplayStateTransitionFailure.NotInitialized);

            return _controller.TryTransition(target, reason);
        }
    }
}
