namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnPlacementResult
    {
        public bool IsValid { get; }
        public SpawnPoint ResolvedPoint { get; }
        public SpawnRejectReason RejectReason { get; }
        private SpawnPlacementResult(bool valid, in SpawnPoint point, SpawnRejectReason reason) { IsValid=valid; ResolvedPoint=point; RejectReason=reason; }
        public static SpawnPlacementResult Accepted(in SpawnPoint point) => new SpawnPlacementResult(true,in point,SpawnRejectReason.None);
        public static SpawnPlacementResult Rejected(SpawnRejectReason reason) { SpawnPoint p=default; return new SpawnPlacementResult(false,in p,reason); }
    }
}
