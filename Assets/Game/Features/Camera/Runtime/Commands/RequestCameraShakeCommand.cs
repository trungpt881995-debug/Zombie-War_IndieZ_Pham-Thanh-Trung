using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Commands
{
    public readonly struct RequestCameraShakeCommand : ICommand
    {
        public CameraShakeId ShakeId { get; }
        public RequestCameraShakeCommand(CameraShakeId shakeId) => ShakeId = shakeId;
    }
}
