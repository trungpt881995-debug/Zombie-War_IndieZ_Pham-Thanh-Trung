using System; using System.Collections.Generic; using GameplayCore.Entities; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports; using ZombieWar.Features.VFX.Services; using ZombieWar.Features.Weapon.Domain; using ZombieWar.Features.Weapon.Ports;
namespace ZombieWar.Integration.VFX.Weapon
{
    public sealed class WeaponVFXFeedbackPort:IWeaponFeedbackPort
    {
        private readonly IVFXRuntime _vfx; private readonly IWeaponMuzzleProvider _muzzles; private readonly Dictionary<EntityId,VFXHandle> _flames=new Dictionary<EntityId,VFXHandle>(4);
        public WeaponVFXFeedbackPort(IVFXRuntime vfx,IWeaponMuzzleProvider muzzles){_vfx=vfx??throw new ArgumentNullException(nameof(vfx));_muzzles=muzzles??throw new ArgumentNullException(nameof(muzzles));}
        public void OnShotFired(EntityId ownerId,WeaponType weapon){if(!TryPose(ownerId,out VFXPose pose))return;var req=new VFXRequest(Map(weapon),in pose);_vfx.Play(in req);}
        public void OnFlameStarted(EntityId ownerId){if(_flames.ContainsKey(ownerId))return;if(!TryPose(ownerId,out VFXPose pose))return;var anchor=new MuzzleAnchor(ownerId,_muzzles);var req=new VFXRequest(VFXId.FlamethrowerLoop,in pose,0f,anchor);var h=_vfx.Play(in req);if(h.IsValid)_flames[ownerId]=h;}
        public void OnFlameStopped(EntityId ownerId){if(_flames.TryGetValue(ownerId,out VFXHandle h)){_vfx.Stop(h);_flames.Remove(ownerId);}}
        private bool TryPose(EntityId owner,out VFXPose pose){if(!_muzzles.TryGetMuzzle(owner,out WeaponMuzzle m)){pose=default;return false;}var p=new VFXPoint(m.Position.X,m.Position.Y,m.Position.Z);var d=new VFXDirection(m.Forward.X,m.Forward.Y,m.Forward.Z);pose=new VFXPose(in p,in d);return true;}
        private static VFXId Map(WeaponType w){switch(w){case WeaponType.Pistol:return VFXId.PistolMuzzle;case WeaponType.AK:return VFXId.AKMuzzle;case WeaponType.Shotgun:return VFXId.ShotgunMuzzle;case WeaponType.SniperRifle:return VFXId.SniperMuzzle;case WeaponType.GrenadeLauncher:return VFXId.GrenadeMuzzle;default:return VFXId.None;}}
        private sealed class MuzzleAnchor:IVFXAnchor{private readonly EntityId _owner;private readonly IWeaponMuzzleProvider _m;public MuzzleAnchor(EntityId owner,IWeaponMuzzleProvider m){_owner=owner;_m=m;}public bool IsValid=>_m.TryGetMuzzle(_owner,out _);public VFXPose Pose{get{_m.TryGetMuzzle(_owner,out WeaponMuzzle x);var p=new VFXPoint(x.Position.X,x.Position.Y,x.Position.Z);var d=new VFXDirection(x.Forward.X,x.Forward.Y,x.Forward.Z);return new VFXPose(in p,in d);}}}
    }
}
