using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using GameplayCore.Targeting;
using ZombieWar.Features.Targeting.Controller;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Model;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Targeting.Selection;
using ZombieWar.Features.Targeting.View;

namespace ZombieWar.Features.Targeting.Factories
{
    /// <summary>
    /// Factory Pattern: shared selector/validator/registry are reused, while each
    /// Soldier receives an independent TargetingModel + TargetingController session.
    /// </summary>
    public sealed class TargetingFactory : ITargetingFactory
    {
        private readonly ITargetRegistry _registry;
        private readonly ITargetSelector<TargetingContext, ITargetCandidate> _selector;
        private readonly ITargetValidator _validator;
        private readonly IEventBus _eventBus;

        public TargetingFactory(ITargetRegistry registry, ITargetSelector<TargetingContext, ITargetCandidate> selector, ITargetValidator validator, IEventBus eventBus)
        {
            _registry = registry ??
                throw new ArgumentNullException(nameof(registry));

            _selector = selector ??
                throw new ArgumentNullException(nameof(selector));

            _validator = validator ??
                throw new ArgumentNullException(nameof(validator));

            _eventBus = eventBus ??
                throw new ArgumentNullException(nameof(eventBus));
        }

        public ITargetingSession Create(EntityId ownerId, ITargetingView view = null)
        {
            return new TargetingController(ownerId, new TargetingModel(), _registry, _selector, _validator, view ?? NullTargetingView.Instance, _eventBus);
        }
    }
}
