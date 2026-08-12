using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.View;

namespace ZombieWar.Features.Targeting.Factories
{
    public interface ITargetingFactory
    {
        ITargetingSession Create(EntityId ownerId, ITargetingView view = null);
    }
}
