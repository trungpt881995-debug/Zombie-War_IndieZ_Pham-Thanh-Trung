namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapLoadResult
    {
        public bool Success { get; }
        public bool AlreadyLoaded { get; }
        public MapId MapId { get; }
        public MapLoadFailureReason FailureReason { get; }
        public string Message { get; }

        private MapLoadResult(bool success, bool alreadyLoaded, MapId mapId, MapLoadFailureReason failureReason, string message)
        {
            Success = success;
            AlreadyLoaded = alreadyLoaded;
            MapId = mapId;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
        }

        public static MapLoadResult Loaded(MapId mapId) => new MapLoadResult(true, false, mapId, MapLoadFailureReason.None, string.Empty);
        public static MapLoadResult Already(MapId mapId) => new MapLoadResult(true, true, mapId, MapLoadFailureReason.None, string.Empty);
        public static MapLoadResult Failed(MapId mapId, MapLoadFailureReason reason, string message = "") => new MapLoadResult(false, false, mapId, reason, message);
    }
}
