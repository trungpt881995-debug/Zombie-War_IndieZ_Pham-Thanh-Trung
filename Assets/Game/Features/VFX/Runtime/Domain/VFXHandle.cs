using System;
namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXHandle : IEquatable<VFXHandle>
    {
        public long Value { get; }
        public bool IsValid => Value>0;
        public VFXHandle(long value){Value=value;}
        public bool Equals(VFXHandle other)=>Value==other.Value;
        public override bool Equals(object obj)=>obj is VFXHandle other&&Equals(other);
        public override int GetHashCode()=>Value.GetHashCode();
        public override string ToString()=>Value.ToString();
        public static bool operator==(VFXHandle a,VFXHandle b)=>a.Equals(b);
        public static bool operator!=(VFXHandle a,VFXHandle b)=>!a.Equals(b);
    }
}
