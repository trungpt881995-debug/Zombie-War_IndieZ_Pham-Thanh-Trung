using System; using System.Collections.Generic; using ZombieWar.Features.VFX.Catalog; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Model; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Controller
{
    public sealed class VFXController
    {
        private readonly VFXModel _model; private readonly List<VFXInstanceModel> _active=new List<VFXInstanceModel>(64); private readonly Dictionary<long,VFXInstanceModel> _byHandle=new Dictionary<long,VFXInstanceModel>(64);
        private IVFXCatalog _catalog; private IVFXPoolRegistry _pools;
        public VFXController(VFXModel model){_model=model??throw new ArgumentNullException(nameof(model));}
        public bool IsInitialized=>_model.IsInitialized; public VFXGameplayMode Mode=>_model.Mode; public int ActiveCount=>_active.Count;
        public VFXSnapshot Snapshot=>new VFXSnapshot(IsInitialized,Mode,ActiveCount,_model.PlayedCount,_model.RejectedCount);
        public void Initialize(IVFXCatalog catalog,IVFXPoolRegistry pools){if(catalog==null)throw new ArgumentNullException(nameof(catalog));if(pools==null)throw new ArgumentNullException(nameof(pools));if(IsInitialized)Shutdown();_catalog=catalog;_pools=pools;_model.IsInitialized=true;ApplyPauseState();}
        public void Shutdown(){CancelAll();_pools?.ReleaseAll();_catalog=null;_pools=null;_model.IsInitialized=false;}
        public VFXHandle Play(in VFXRequest request)
        {
            if(!IsInitialized||!CanAccept(request.Id)){_model.RejectedCount++;return default;}
            if(!_catalog.TryGet(request.Id,out VFXDefinition definition)){_model.RejectedCount++;return default;}
            if(Mode==VFXGameplayMode.TerminalDrain&&(definition.Lifetime!=VFXLifetimeMode.OneShot||!definition.AllowDuringTerminalDrain)){_model.RejectedCount++;return default;}
            if(!_pools.TryAcquire(request.Id,out IVFXLease lease)||lease==null||lease.View==null){_model.RejectedCount++;return default;}
            long value=_model.NextHandle++; if(value<=0){_model.NextHandle=2;value=1;}
            var handle=new VFXHandle(value); var inst=new VFXInstanceModel{Handle=handle,Definition=definition,Lease=lease,Anchor=request.Anchor,Elapsed=0f,Index=_active.Count,Paused=Mode==VFXGameplayMode.Suspended};
            VFXPose pose=request.Anchor!=null&&request.Anchor.IsValid?request.Anchor.Pose:request.Pose; float scale=request.Scale>0f?request.Scale:definition.DefaultScale;
            lease.View.Activate(in pose,scale); lease.View.Play(); lease.View.SetPaused(inst.Paused); _active.Add(inst); _byHandle.Add(value,inst); _model.PlayedCount++; return handle;
        }
        public bool Stop(VFXHandle handle){if(!handle.IsValid||!_byHandle.TryGetValue(handle.Value,out VFXInstanceModel inst))return false;Release(inst,true);return true;}
        public void SetMode(VFXGameplayMode mode)
        {
            if(mode< VFXGameplayMode.Inactive||mode>VFXGameplayMode.TerminalDrain)throw new ArgumentOutOfRangeException(nameof(mode));
            if(_model.Mode==mode)return; _model.Mode=mode;
            if(mode==VFXGameplayMode.Inactive){CancelAll();return;}
            if(mode==VFXGameplayMode.TerminalDrain){for(int i=_active.Count-1;i>=0;i--)if(_active[i].Definition.Lifetime==VFXLifetimeMode.Looping)Release(_active[i],true);}
            ApplyPauseState();
        }
        public void Tick(float deltaTime)
        {
            if(!IsInitialized||Mode==VFXGameplayMode.Inactive||Mode==VFXGameplayMode.Suspended)return; if(float.IsNaN(deltaTime)||float.IsInfinity(deltaTime)||deltaTime<0f)throw new ArgumentOutOfRangeException(nameof(deltaTime));
            for(int i=_active.Count-1;i>=0;i--){var inst=_active[i];if(inst.Anchor!=null&&inst.Anchor.IsValid){var p=inst.Anchor.Pose;inst.Lease.View.SetPose(in p);}if(inst.Definition.Lifetime==VFXLifetimeMode.OneShot){inst.Elapsed+=deltaTime;if(inst.Elapsed>=inst.Definition.Duration)Release(inst,false);}}
        }
        public void CancelAll(){for(int i=_active.Count-1;i>=0;i--)Release(_active[i],true);}
        private bool CanAccept(VFXId id){return Mode==VFXGameplayMode.Playing||Mode==VFXGameplayMode.TerminalDrain;}
        private void ApplyPauseState(){bool pause=_model.Mode==VFXGameplayMode.Suspended;for(int i=0;i<_active.Count;i++){var inst=_active[i];if(inst.Paused==pause)continue;inst.Paused=pause;inst.Lease.View.SetPaused(pause);}}
        private void Release(VFXInstanceModel inst,bool stop)
        {
            if(inst==null||inst.Lease==null)return; if(stop)inst.Lease.View.Stop(); inst.Lease.View.Deactivate(); inst.Lease.Release(); _byHandle.Remove(inst.Handle.Value);
            int idx=inst.Index,last=_active.Count-1;if(idx<0||idx>last)return;if(idx!=last){var moved=_active[last];_active[idx]=moved;moved.Index=idx;}_active.RemoveAt(last);inst.Index=-1;
        }
    }
}
