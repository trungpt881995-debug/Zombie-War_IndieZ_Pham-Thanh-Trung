using System;
using ZombieWar.Features.VFX.Domain;

namespace ZombieWar.Features.VFX.Services
{
    public interface IVFXRuntime
    {
        event Action<VFXHandle> Completed;

        bool IsInitialized { get; }
        VFXGameplayMode Mode { get; }
        int ActiveCount { get; }
        VFXSnapshot Snapshot { get; }

        VFXHandle Play(in VFXRequest request);
        bool Stop(VFXHandle handle);
        bool IsActive(VFXHandle handle);
        void SetMode(VFXGameplayMode mode);
        void Tick(float deltaTime);
        void CancelAll();
    }
}
