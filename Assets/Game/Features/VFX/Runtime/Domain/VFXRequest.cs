using System; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXRequest
    {
        public VFXId Id{get;} public VFXPose Pose{get;} public float Scale{get;} public IVFXAnchor Anchor{get;}
        public VFXRequest(VFXId id,in VFXPose pose,float scale=0f,IVFXAnchor anchor=null){if(id==VFXId.None)throw new ArgumentOutOfRangeException(nameof(id));if(float.IsNaN(scale)||float.IsInfinity(scale)||scale<0f)throw new ArgumentOutOfRangeException(nameof(scale));Id=id;Pose=pose;Scale=scale;Anchor=anchor;}
    }
}
