using System;
namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossPoint : IEquatable<BossPoint>
    {
        public float X { get; } public float Y { get; } public float Z { get; }
        public BossPoint(float x,float y,float z){ if(float.IsNaN(x)||float.IsNaN(y)||float.IsNaN(z)||float.IsInfinity(x)||float.IsInfinity(y)||float.IsInfinity(z)) throw new ArgumentOutOfRangeException(nameof(x)); X=x;Y=y;Z=z; }
        public BossPoint Add(float x,float y,float z)=>new BossPoint(X+x,Y+y,Z+z);
        public static float SqrDistanceXZ(in BossPoint a,in BossPoint b){float x=b.X-a.X,z=b.Z-a.Z;return x*x+z*z;}
        public bool Equals(BossPoint o)=>X.Equals(o.X)&&Y.Equals(o.Y)&&Z.Equals(o.Z);
        public override bool Equals(object obj)=>obj is BossPoint o&&Equals(o);
        public override int GetHashCode()=>HashCode.Combine(X,Y,Z);
    }
}
