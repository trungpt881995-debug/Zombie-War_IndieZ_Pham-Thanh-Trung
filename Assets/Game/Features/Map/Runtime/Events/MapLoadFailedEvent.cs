using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Events
{
    public readonly struct MapLoadFailedEvent : IEvent
    {
        public MapId MapId { get; }
        public MapLoadFailureReason Reason { get; }
        public string Message { get; }

        public MapLoadFailedEvent(MapId mapId, MapLoadFailureReason reason, string message)
        {
            MapId = mapId;
            Reason = reason;
            Message = message ?? string.Empty;
        }
    }
}
