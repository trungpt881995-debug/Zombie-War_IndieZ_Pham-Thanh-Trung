using System; using GeneralCore.Architecture; using GameplayCore.Entities; using ZombieWar.Features.Health.Events; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Integration.VFX.Soldier
{
    public sealed class SoldierDamageVFXBridge:ISoldierVFXAnchorBinding,IDisposable
    {
        private readonly IEventSubscriber _events; private readonly IVFXRuntime _vfx; private IDisposable _sub; private EntityId _group; private IVFXAnchor _anchor;
        public SoldierDamageVFXBridge(IEventSubscriber events,IVFXRuntime vfx){_events=events??throw new ArgumentNullException(nameof(events));_vfx=vfx??throw new ArgumentNullException(nameof(vfx));}
        public void Start(){if(_sub==null)_sub=_events.Subscribe<HealthChangedEvent>(OnHealth);}
        public void Bind(EntityId groupId,IVFXAnchor anchor){_group=groupId;_anchor=anchor;}
        public void Unbind(EntityId groupId){if(_group.Equals(groupId)){_group=default;_anchor=null;}}
        private void OnHealth(HealthChangedEvent e){if(_anchor==null||!_anchor.IsValid||!e.OwnerId.Equals(_group)||e.CurrentHealth>=e.PreviousHealth)return;var pose=_anchor.Pose;var r=new VFXRequest(VFXId.SoldierDamage,in pose);_vfx.Play(in r);}
        public void Dispose(){_sub?.Dispose();_sub=null;_anchor=null;_group=default;}
    }
}
