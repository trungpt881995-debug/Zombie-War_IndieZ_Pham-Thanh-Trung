using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Events
{
    public readonly struct MapLoadedEvent : IEvent
    {
        public MapId MapId { get; }
        public MapLoadedEvent(MapId mapId) => MapId = mapId;
    }
}
