using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Health.Controller;
using ZombieWar.Features.Health.Model;
using ZombieWar.Features.Health.View;

namespace ZombieWar.Features.Health.Factories
{
    /// <summary>
    /// Factory Pattern: centralizes creation so Soldier/Zombie/Boss Features do not
    /// duplicate HealthModel + HealthController construction logic.
    /// </summary>
    public sealed class HealthFactory : IHealthFactory
    {
        private readonly IEventBus _eventBus;

        public HealthFactory(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
        }

        public HealthController Create(
            EntityId ownerId,
            float maxHealth,
            IHealthView view = null)
        {
            var model = new HealthModel(maxHealth);

            return new HealthController(
                ownerId,
                model,
                view ?? NullHealthView.Instance,
                _eventBus);
        }
    }
}
