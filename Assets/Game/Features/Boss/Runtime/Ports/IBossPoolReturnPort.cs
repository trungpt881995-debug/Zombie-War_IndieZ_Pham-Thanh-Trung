using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Ports
{
    public interface IBossPoolReturnPort
    {
        void Return(EntityId entityId, BossReleaseReason reason);
    }
}
