using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Events
{
    public readonly struct CameraProfileChangedEvent : IEvent
    {
        public CameraProfile Profile { get; }
        public CameraProfileChangedEvent(in CameraProfile profile) => Profile = profile;
    }
}
