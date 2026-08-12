using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using GameplayCore.Targeting;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Events;
using ZombieWar.Features.Targeting.Model;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Targeting.Selection;
using ZombieWar.Features.Targeting.View;

namespace ZombieWar.Features.Targeting.Controller
{
    /// <summary>
    /// MVC Controller: orchestrates retention, validation, reacquisition,
    /// presentation and events. One instance belongs to one Soldier.
    /// </summary>
    public sealed class TargetingController : IController, ITargetingSession
    {
        private readonly EntityId _ownerId;
        private readonly TargetingModel _model;
        private readonly ITargetRegistry _registry;
        private readonly ITargetSelector<TargetingContext, ITargetCandidate> _selector;
        private readonly ITargetValidator _validator;
        private readonly ITargetingView _view;
        private readonly IEventBus _eventBus;

        public TargetingController(EntityId ownerId, TargetingModel model, ITargetRegistry registry, ITargetSelector<TargetingContext, ITargetCandidate> selector, ITargetValidator validator, ITargetingView view, IEventBus eventBus)
        {
            _ownerId = ownerId;

            _model = model ??
                throw new ArgumentNullException(nameof(model));

            _registry = registry ??
                throw new ArgumentNullException(nameof(registry));

            _selector = selector ??
                throw new ArgumentNullException(nameof(selector));

            _validator = validator ??
                throw new ArgumentNullException(nameof(validator));

            _view = view ?? NullTargetingView.Instance;

            _eventBus = eventBus ??
                throw new ArgumentNullException(nameof(eventBus));
        }

        public TargetingResult Evaluate(in TargetingContext context)
        {
            TargetLossReason lastLossReason = TargetLossReason.None;

            if (_model.HasTarget)
            {
                TargetHandle current = _model.CurrentTarget;

                TargetLossReason reason = _validator.Validate(in current, in context);

                if (reason == TargetLossReason.None)
                {
                    TargetingResult retained = TargetingResult.From(in current);

                    Render(in retained, TargetLossReason.None);

                    return retained;
                }

                LoseCurrent(reason);
                lastLossReason = reason;
            }

            ITargetCandidate candidate = _selector.Select(context, _registry.ActiveTargets);

            if (candidate == null)
            {
                TargetingResult none = TargetingResult.None;

                Render(in none, lastLossReason);

                return none;
            }

            var handle = new TargetHandle(candidate);

            // Safety validation at acquisition time. In a correctly managed pool
            // this should always pass; it also protects against stale registry entries.
            TargetLossReason acquireValidation = _validator.Validate(in handle, in context);

            if (acquireValidation != TargetLossReason.None)
            {
                TargetingResult none = TargetingResult.None;

                Render(in none, acquireValidation);

                return none;
            }

            _model.Acquire(in handle);

            _eventBus.Publish(new TargetAcquiredEvent(_ownerId, handle.EntityId));

            TargetingResult acquired = TargetingResult.From(in handle);

            Render(in acquired, TargetLossReason.None);

            return acquired;
        }

        public void Clear(TargetLossReason reason = TargetLossReason.ManualClear)
        {
            if (!_model.HasTarget)
                return;

            LoseCurrent(reason);

            TargetingResult none = TargetingResult.None;

            Render(in none, reason);
        }

        private void LoseCurrent(TargetLossReason reason)
        {
            TargetHandle lost = _model.CurrentTarget;

            _model.Clear();

            _eventBus.Publish(new TargetLostEvent(_ownerId, lost.EntityId, reason));
        }

        private void Render(in TargetingResult result, TargetLossReason lossReason)
        {
            var state = new TargetingViewState(_ownerId, in result, lossReason);

            _view.Render(in state);
        }
    }
}
