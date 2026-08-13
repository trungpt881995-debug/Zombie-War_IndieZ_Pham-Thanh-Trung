using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Model;
using ZombieWar.Features.GameState.Policies;

namespace ZombieWar.Features.GameState.Controller
{
    public sealed class GameStateController : IController
    {
        private readonly GameStateModel _model;
        private readonly IGameplayStateTransitionPolicy _policy;
        private readonly IEventBus _events;

        public GameStateController(
            GameStateModel model,
            IGameplayStateTransitionPolicy policy,
            IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public GameplayStateTransitionResult TryTransition(
            GameplayStateId target,
            GameplayStateTransitionReason reason)
        {
            GameplayStateId current = _model.Current;
            if (current == target)
                return GameplayStateTransitionResult.Rejected(current, reason, GameplayStateTransitionFailure.SameState);

            if (!_policy.CanTransition(current, target))
                return GameplayStateTransitionResult.Rejected(current, reason, GameplayStateTransitionFailure.InvalidTransition);

            _model.Commit(target);
            _events.Publish(new GameplayStateChangedEvent(
                current,
                target,
                reason,
                _model.TransitionSequence));

            return GameplayStateTransitionResult.AcceptedTransition(current, target, reason);
        }
    }
}
