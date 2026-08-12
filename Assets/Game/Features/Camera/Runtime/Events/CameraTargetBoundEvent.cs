using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Events
{
    public readonly struct CameraTargetBoundEvent : IEvent
    {
        public CameraPoint Target { get; }
        public CameraTargetBoundEvent(in CameraPoint target) => Target = target;
    }
}
