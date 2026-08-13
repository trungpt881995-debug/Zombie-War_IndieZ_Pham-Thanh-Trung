using System; using GeneralCore.Architecture; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Presentation; using ZombieWar.Features.Weapon.Domain; using ZombieWar.Features.Weapon.Events; using ZombieWar.Features.Weapon.Services;
namespace ZombieWar.Integration.UI.Weapon
{
    public sealed class WeaponUIBridge:IDisposable
    { private static readonly WeaponType[] Types={WeaponType.Pistol,WeaponType.AK,WeaponType.Shotgun,WeaponType.SniperRifle,WeaponType.GrenadeLauncher,WeaponType.Flamethrower}; private readonly IWeaponRuntime _runtime; private readonly WeaponHudPresenter _presenter; private readonly IEventSubscriber _events; private IDisposable _selected;
      public WeaponUIBridge(IWeaponRuntime runtime,WeaponHudPresenter presenter,IEventSubscriber events){_runtime=runtime;_presenter=presenter;_events=events;}
      public void Start(){PresentSelected();_selected=_events.Subscribe<WeaponSelectedEvent>(e=>_presenter.PresentSelected(Map(e.Current)));Tick();}
      public void Tick(){if(!_runtime.IsInitialized)return;var cooldowns=_runtime.Cooldowns;for(int i=0;i<Types.Length;i++){var type=Types[i];float remaining=cooldowns.Get(type);float duration=0f;if(_runtime.TryGetDefinition(type,out var def))duration=def.SelectionCooldown;float normalized=duration>0f?remaining/duration:0f;bool interactable=_runtime.GameplayEnabled&&remaining<=0f&&type!=_runtime.CurrentWeapon;_presenter.PresentWeapon(Map(type),normalized,interactable);}}
      public void Dispose(){_selected?.Dispose();_selected=null;} private void PresentSelected(){if(_runtime.IsInitialized)_presenter.PresentSelected(Map(_runtime.CurrentWeapon));}
      private static UIWeaponId Map(WeaponType t){switch(t){case WeaponType.Pistol:return UIWeaponId.Pistol;case WeaponType.AK:return UIWeaponId.AK;case WeaponType.Shotgun:return UIWeaponId.Shotgun;case WeaponType.SniperRifle:return UIWeaponId.SniperRifle;case WeaponType.GrenadeLauncher:return UIWeaponId.GrenadeLauncher;case WeaponType.Flamethrower:return UIWeaponId.Flamethrower;default:throw new ArgumentOutOfRangeException(nameof(t));}}
    }
}
