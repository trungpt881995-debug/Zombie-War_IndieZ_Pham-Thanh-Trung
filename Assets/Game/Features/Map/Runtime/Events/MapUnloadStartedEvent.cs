using GeneralCore.Architecture;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Events
{
    public readonly struct MapUnloadStartedEvent : IEvent
    {
        public MapId MapId { get; }
        public MapUnloadStartedEvent(MapId mapId) => MapId = mapId;
    }
}
