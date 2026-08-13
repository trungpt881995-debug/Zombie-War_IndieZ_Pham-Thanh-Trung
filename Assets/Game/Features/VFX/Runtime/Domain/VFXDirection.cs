using System;
namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXDirection
    {
        public float X{get;} public float Y{get;} public float Z{get;}
        public VFXDirection(float x,float y,float z)
        {
            if(float.IsNaN(x)||float.IsInfinity(x)||float.IsNaN(y)||float.IsInfinity(y)||float.IsNaN(z)||float.IsInfinity(z))throw new ArgumentOutOfRangeException();
            float sq=x*x+y*y+z*z; if(sq<=0.000001f)throw new ArgumentException("Direction must be non-zero.");
            float inv=1f/(float)Math.Sqrt(sq);X=x*inv;Y=y*inv;Z=z*inv;
        }
        public static VFXDirection Forward=>new VFXDirection(0f,0f,1f);
    }
}
