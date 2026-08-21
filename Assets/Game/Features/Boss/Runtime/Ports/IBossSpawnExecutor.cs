using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Ports
{
    public interface IBossSpawnExecutor
    {
        bool TrySpawnPlan(in BossSpawnPlan plan);
    }
}
