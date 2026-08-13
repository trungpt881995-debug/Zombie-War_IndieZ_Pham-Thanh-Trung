using System; using GameplayCore.Entities; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Integration.VFX.Boss
{
    public sealed class BossVFXFeedbackPort:IBossFeedbackPort
    {
        private readonly IVFXRuntime _vfx; public BossVFXFeedbackPort(IVFXRuntime vfx)=>_vfx=vfx??throw new ArgumentNullException(nameof(vfx));
        public void OnSpawn(BossId bossId,EntityId entityId,in BossPoint position)=>Play(VFXId.BossSpawn,in position);
        public void OnHit(BossId bossId,EntityId entityId,in BossPoint position)=>Play(VFXId.BossHit,in position);
        public void OnDeath(BossId bossId,EntityId entityId,in BossPoint position)=>Play(VFXId.BossDeath,in position);
        private void Play(VFXId id,in BossPoint z){var p=new VFXPoint(z.X,z.Y,z.Z);var pose=VFXPose.At(in p);var r=new VFXRequest(id,in pose);_vfx.Play(in r);}
    }
}
