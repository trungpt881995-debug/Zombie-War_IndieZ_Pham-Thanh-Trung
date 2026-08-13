using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Unity.Anchors
{
    public sealed class TransformVFXAnchor:MonoBehaviour,IVFXAnchor
    {
        [SerializeField]private Transform target; public bool IsValid=>(target!=null?target:transform)!=null;
        public VFXPose Pose{get{Transform t=target!=null?target:transform;Vector3 p=t.position,f=t.forward;var point=new VFXPoint(p.x,p.y,p.z);var dir=new VFXDirection(f.x,f.y,f.z);return new VFXPose(in point,in dir);}}
    }
}
