using System; using GameplayCore.Entities; using ZombieWar.Features.Projectile.Domain; using ZombieWar.Features.Projectile.Ports; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Integration.VFX.Projectile
{
    public sealed class ProjectileVFXFeedbackPort:IProjectileFeedbackPort
    {
        private readonly IVFXRuntime _vfx; public ProjectileVFXFeedbackPort(IVFXRuntime vfx)=>_vfx=vfx??throw new ArgumentNullException(nameof(vfx));
        public void OnHit(EntityId projectileId,EntityId targetId,in ProjectilePoint point){var p=new VFXPoint(point.X,point.Y,point.Z);var pose=VFXPose.At(in p);var impact=new VFXRequest(VFXId.BulletImpact,in pose);_vfx.Play(in impact);var blood=new VFXRequest(VFXId.BloodImpact,in pose);_vfx.Play(in blood);}
        public void OnExplosion(EntityId projectileId,in ProjectilePoint point,float radius){var p=new VFXPoint(point.X,point.Y,point.Z);var pose=VFXPose.At(in p);var req=new VFXRequest(VFXId.GrenadeExplosion,in pose,radius>0f?Math.Max(1f,radius):1f);_vfx.Play(in req);}
    }
}
