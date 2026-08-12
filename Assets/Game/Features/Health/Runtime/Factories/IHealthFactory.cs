using GameplayCore.Entities;
using ZombieWar.Features.Health.Controller;
using ZombieWar.Features.Health.View;

namespace ZombieWar.Features.Health.Factories
{
    public interface IHealthFactory
    {
        HealthController Create(
            EntityId ownerId,
            float maxHealth,
            IHealthView view = null);
    }
}
