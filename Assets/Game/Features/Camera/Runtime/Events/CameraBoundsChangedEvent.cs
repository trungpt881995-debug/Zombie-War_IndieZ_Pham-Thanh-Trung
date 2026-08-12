using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Events
{
    public readonly struct CameraBoundsChangedEvent : IEvent
    {
        public bool HasBounds { get; }
        public CameraBounds Bounds { get; }

        public CameraBoundsChangedEvent(bool hasBounds, in CameraBounds bounds)
        {
            HasBounds = hasBounds;
            Bounds = bounds;
        }
    }
}
