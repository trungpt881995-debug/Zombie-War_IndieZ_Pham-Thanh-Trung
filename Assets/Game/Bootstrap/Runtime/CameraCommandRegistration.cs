using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Camera.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class CameraCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands;
        private readonly RequestCameraShakeCommandHandler _handler;

        public CameraCommandRegistration(
            ICommandRegistry commands,
            RequestCameraShakeCommandHandler handler)
        {
            _commands = commands;
            _handler = handler;
        }

        public void Start()
        {
            _commands.Register<RequestCameraShakeCommand>(_handler);
        }
    }
}
