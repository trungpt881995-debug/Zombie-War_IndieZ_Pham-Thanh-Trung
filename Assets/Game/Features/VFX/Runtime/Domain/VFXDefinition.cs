using System;
namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXDefinition
    {
        public VFXId Id{get;} public VFXLifetimeMode Lifetime{get;} public float Duration{get;}
        public bool AllowDuringTerminalDrain{get;} public int PrewarmCount{get;} public int MaxCapacity{get;} public bool AllowGrowth{get;} public float DefaultScale{get;}
        public VFXDefinition(VFXId id,VFXLifetimeMode lifetime,float duration,bool terminal,int prewarm,int maxCapacity,bool allowGrowth,float defaultScale)
        {
            if(id==VFXId.None)throw new ArgumentOutOfRangeException(nameof(id));
            if(lifetime==VFXLifetimeMode.OneShot&&(duration<=0f||float.IsNaN(duration)||float.IsInfinity(duration)))throw new ArgumentOutOfRangeException(nameof(duration));
            if(prewarm<0)throw new ArgumentOutOfRangeException(nameof(prewarm)); if(maxCapacity<=0||maxCapacity<prewarm)throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            if(defaultScale<=0f||float.IsNaN(defaultScale)||float.IsInfinity(defaultScale))throw new ArgumentOutOfRangeException(nameof(defaultScale));
            Id=id;Lifetime=lifetime;Duration=lifetime==VFXLifetimeMode.Looping?0f:duration;AllowDuringTerminalDrain=terminal;PrewarmCount=prewarm;MaxCapacity=maxCapacity;AllowGrowth=allowGrowth;DefaultScale=defaultScale;
        }
    }
}
