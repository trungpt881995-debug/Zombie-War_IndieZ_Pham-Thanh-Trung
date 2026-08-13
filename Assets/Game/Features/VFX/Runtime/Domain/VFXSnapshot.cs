namespace ZombieWar.Features.VFX.Domain
{
    public readonly struct VFXSnapshot
    {
        public bool IsInitialized{get;} public VFXGameplayMode Mode{get;} public int ActiveCount{get;} public long PlayedCount{get;} public long RejectedCount{get;}
        public VFXSnapshot(bool initialized,VFXGameplayMode mode,int active,long played,long rejected){IsInitialized=initialized;Mode=mode;ActiveCount=active;PlayedCount=played;RejectedCount=rejected;}
    }
}
