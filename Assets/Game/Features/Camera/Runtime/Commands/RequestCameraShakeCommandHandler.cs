using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Camera.Services;

namespace ZombieWar.Features.Camera.Commands
{
    public sealed class RequestCameraShakeCommandHandler : ICommandHandler<RequestCameraShakeCommand>
    {
        private readonly ICameraRuntime _runtime;
        public RequestCameraShakeCommandHandler(ICameraRuntime runtime) =>
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        public void Handle(RequestCameraShakeCommand command) =>
            _runtime.TryRequestShake(command.ShakeId);
    }
}
