using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Model
{
    public sealed class VFXInstanceModel
    {
        public VFXHandle Handle; public VFXDefinition Definition; public IVFXLease Lease; public IVFXAnchor Anchor; public float Elapsed; public int Index; public bool Paused;
    }
}
