using System;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Integration.Audio.Projectile;
using ZombieWar.Integration.VFX.Projectile;

namespace ZombieWar.Bootstrap
{
    /// <summary>
    /// Composition-only fan-out for Projectile presentation.
    /// Projectile gameplay remains independent from VFX and Audio implementations.
    /// </summary>
    public sealed class ProjectilePresentationFeedbackPort :
        IProjectileFeedbackPort
    {
        private readonly ProjectileVFXFeedbackPort _vfx;
        private readonly ProjectileAudioFeedbackPort _audio;

        public ProjectilePresentationFeedbackPort(
            ProjectileVFXFeedbackPort vfx,
            ProjectileAudioFeedbackPort audio)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public void OnHit(
            EntityId projectileId,
            EntityId targetId,
            in ProjectilePoint point)
        {
            _vfx.OnHit(
                projectileId,
                targetId,
                in point);

            _audio.OnHit(
                projectileId,
                targetId,
                in point);
        }

        public void OnExplosion(
            EntityId projectileId,
            in ProjectilePoint point,
            float radius)
        {
            _vfx.OnExplosion(
                projectileId,
                in point,
                radius);

            _audio.OnExplosion(
                projectileId,
                in point,
                radius);
        }
    }
}
