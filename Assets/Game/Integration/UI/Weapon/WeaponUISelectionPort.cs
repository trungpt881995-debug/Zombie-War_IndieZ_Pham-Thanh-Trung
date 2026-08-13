using System; using GeneralCore.Architecture; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Ports; using ZombieWar.Features.Weapon.Commands; using ZombieWar.Features.Weapon.Domain;
namespace ZombieWar.Integration.UI.Weapon
{
    public sealed class WeaponUISelectionPort:IWeaponSelectionPort
    { private readonly ICommandBus _commands; public WeaponUISelectionPort(ICommandBus commands)=>_commands=commands??throw new ArgumentNullException(nameof(commands)); public void Select(UIWeaponId id)=>_commands.Send(new SelectWeaponCommand(Map(id)));
      internal static WeaponType Map(UIWeaponId id){switch(id){case UIWeaponId.Pistol:return WeaponType.Pistol;case UIWeaponId.AK:return WeaponType.AK;case UIWeaponId.Shotgun:return WeaponType.Shotgun;case UIWeaponId.SniperRifle:return WeaponType.SniperRifle;case UIWeaponId.GrenadeLauncher:return WeaponType.GrenadeLauncher;case UIWeaponId.Flamethrower:return WeaponType.Flamethrower;default:throw new ArgumentOutOfRangeException(nameof(id));}} }
}
