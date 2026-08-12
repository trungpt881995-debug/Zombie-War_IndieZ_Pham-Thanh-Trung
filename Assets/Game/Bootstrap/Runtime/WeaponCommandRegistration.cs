using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Weapon.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class WeaponCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _commands;
        private readonly SelectWeaponCommandHandler _handler;

        public WeaponCommandRegistration(
            ICommandRegistry commands,
            SelectWeaponCommandHandler handler)
        {
            _commands = commands;
            _handler = handler;
        }

        public void Start()
        {
            _commands.Register<SelectWeaponCommand>(_handler);
        }
    }
}
