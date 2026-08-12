using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Services;

namespace ZombieWar.Features.Weapon.Commands
{
    public sealed class SelectWeaponCommandHandler : ICommandHandler<SelectWeaponCommand>
    {
        private readonly IWeaponRuntime _runtime;
        public SelectWeaponCommandHandler(IWeaponRuntime runtime) => _runtime = runtime;
        public void Handle(SelectWeaponCommand command) => _runtime.TrySelect(command.Weapon);
    }
}
