using System;
namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXPoint
    {
        public float X{get;} public float Y{get;} public float Z{get;}
        public VFXPoint(float x,float y,float z){Finite(x,nameof(x));Finite(y,nameof(y));Finite(z,nameof(z));X=x;Y=y;Z=z;}
        private static void Finite(float v,string n){if(float.IsNaN(v)||float.IsInfinity(v))throw new ArgumentOutOfRangeException(n);}
        public static VFXPoint Zero=>new VFXPoint(0f,0f,0f);
    }
}
