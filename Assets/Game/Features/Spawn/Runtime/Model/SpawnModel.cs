using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Model
{
    public sealed class SpawnModel
    {
        public SpawnState State { get; private set; } = SpawnState.Uninitialized;
        public SpawnDifficultyKey Difficulty { get; private set; }
        public SpawnTuning Tuning { get; private set; }
        public float Elapsed { get; private set; }
        public SpawnStopReason StopReason { get; private set; }
        public SpawnBatchResult LastBatch { get; private set; }
        public int SuccessfulSpawnCount { get; private set; }
        public int RejectedSpawnCount { get; private set; }
        public void Initialize(in SpawnDifficultyKey key,in SpawnTuning tuning) { Difficulty=key; Tuning=tuning; Elapsed=0f; StopReason=SpawnStopReason.None; LastBatch=default; SuccessfulSpawnCount=0; RejectedSpawnCount=0; State=SpawnState.Ready; }
        public void SetDifficulty(in SpawnDifficultyKey key,in SpawnTuning tuning) { Difficulty=key; Tuning=tuning; Elapsed=0f; }
        public void Start() { State=SpawnState.Running; StopReason=SpawnStopReason.None; }
        public void Suspend() { if(State==SpawnState.Running) State=SpawnState.Suspended; }
        public void Resume() { if(State==SpawnState.Suspended) State=SpawnState.Running; }
        public void Stop(SpawnStopReason reason) { State=SpawnState.Stopped; StopReason=reason; Elapsed=0f; }
        public void Advance(float deltaTime) { Elapsed += deltaTime; }
        public bool IntervalReady => Elapsed >= Tuning.Interval;
        public void ConsumeInterval() { Elapsed=0f; }
        public void RecordBatch(in SpawnBatchResult result) { LastBatch=result; SuccessfulSpawnCount += result.Spawned; if(result.Spawned < result.CapacityLimited) RejectedSpawnCount += result.CapacityLimited-result.Spawned; }
        public void Reset() { State=SpawnState.Uninitialized; Difficulty=default; Tuning=default; Elapsed=0f; StopReason=SpawnStopReason.None; LastBatch=default; SuccessfulSpawnCount=0; RejectedSpawnCount=0; }
    }
}
