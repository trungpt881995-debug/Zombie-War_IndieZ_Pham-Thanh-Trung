using System; using GameplayCore.Entities; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Services; using ZombieWar.Features.Zombie.Domain; using ZombieWar.Features.Zombie.Ports;
namespace ZombieWar.Integration.VFX.Zombie
{
    public sealed class ZombieVFXFeedbackPort:IZombieFeedbackPort
    {
        private readonly IVFXRuntime _vfx; public ZombieVFXFeedbackPort(IVFXRuntime vfx)=>_vfx=vfx??throw new ArgumentNullException(nameof(vfx));
        public void OnHit(EntityId zombieId,in ZombiePoint position)=>Play(VFXId.ZombieHit,in position);
        public void OnDeath(EntityId zombieId,in ZombiePoint position)=>Play(VFXId.ZombieDeath,in position);
        private void Play(VFXId id,in ZombiePoint z){var p=new VFXPoint(z.X,z.Y,z.Z);var pose=VFXPose.At(in p);var r=new VFXRequest(id,in pose);_vfx.Play(in r);}
    }
}
