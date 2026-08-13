namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnBatchResult
    {
        public int Desired { get; }
        public int CapacityLimited { get; }
        public int Spawned { get; }
        public SpawnRejectReason LastRejectReason { get; }
        public SpawnBatchResult(int desired,int capacityLimited,int spawned,SpawnRejectReason lastRejectReason)
        { Desired=desired; CapacityLimited=capacityLimited; Spawned=spawned; LastRejectReason=lastRejectReason; }
    }
}
