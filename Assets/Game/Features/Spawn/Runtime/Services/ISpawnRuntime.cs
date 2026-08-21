using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Services
{
    public interface ISpawnRuntime
    {
        bool IsInitialized { get; } SpawnState State { get; } 
        bool GameplayEnabled { get; }
        SpawnDifficultyKey Difficulty { get; } 
        SpawnTuning Tuning { get; } 
        float Elapsed { get; } 
        SpawnStopReason StopReason { get; }
        SpawnBatchResult LastBatch { get; } 
        int SuccessfulSpawnCount { get; } 
        int RejectedSpawnCount { get; }
        void Tick(float deltaTime); 
        void Start(); 
        void SetGameplayEnabled(bool enabled); 
        void Stop(SpawnStopReason reason); 
        bool SetDifficulty(in SpawnDifficultyKey key);
    }
}
