using ZombieWar.Features.VFX.Domain;
namespace ZombieWar.Features.VFX.Model
{
    public sealed class VFXModel
    {
        public bool IsInitialized{get;internal set;} public VFXGameplayMode Mode{get;internal set;}=VFXGameplayMode.Inactive;
        public long PlayedCount{get;internal set;} public long RejectedCount{get;internal set;} public long NextHandle{get;internal set;}=1;
    }
}
