using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Events
{
    public readonly struct MapLoadStartedEvent : IEvent
    {
        public MapId MapId { get; }
        public MapLoadStartedEvent(MapId mapId) => MapId = mapId;
    }
}
