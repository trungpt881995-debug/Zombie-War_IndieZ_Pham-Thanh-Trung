using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Services
{
    public interface IBossRuntime
    {
        bool IsInitialized
        {
            get;
        }
        int ActiveCount
        {
            get;
        }
        bool TrySpawn(in BossSpawnSelection selection, in BossPoint anchor);
        void SetGameplayEnabled(bool enabled);
        void CancelAll();
    }
}
