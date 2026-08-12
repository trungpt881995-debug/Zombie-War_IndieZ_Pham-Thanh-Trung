using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Events
{
    public readonly struct MapUnloadedEvent : IEvent
    {
        public MapId MapId { get; }
        public MapUnloadedEvent(MapId mapId) => MapId = mapId;
    }
}
