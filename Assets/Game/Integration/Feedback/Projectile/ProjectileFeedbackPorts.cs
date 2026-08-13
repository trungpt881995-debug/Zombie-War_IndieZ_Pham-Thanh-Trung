using System;
using GameplayCore.Entities;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Integration.VFX.Projectile;

namespace ZombieWar.Integration.Feedback.Projectile
{
    public sealed class ProjectileGameFeelFeedbackPort : IProjectileFeedbackPort
    {
        private readonly IFeedbackRuntime _feedback;

        public ProjectileGameFeelFeedbackPort(IFeedbackRuntime feedback)
        {
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        public void OnHit(
            EntityId projectileId,
            EntityId targetId,
            in ProjectilePoint point)
        {
        }

        public void OnExplosion(
            EntityId projectileId,
            in ProjectilePoint point,
            float radius)
        {
            float intensity = radius > 0f
                ? Math.Min(
                    1.5f,
                    Math.Max(
                        0.5f,
                        radius / 4f))
                : 1f;

            var request = new FeedbackRequest(
                FeedbackId.GrenadeExplosion,
                intensity,
                projectileId.Value);

            _feedback.Play(in request);
        }
    }

    public sealed class CompositeProjectileFeedbackPort : IProjectileFeedbackPort
    {
        private readonly ProjectileVFXFeedbackPort _vfx;
        private readonly ProjectileGameFeelFeedbackPort _gameFeel;

        public CompositeProjectileFeedbackPort(
            ProjectileVFXFeedbackPort vfx,
            ProjectileGameFeelFeedbackPort gameFeel)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _gameFeel = gameFeel ?? throw new ArgumentNullException(nameof(gameFeel));
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

            _gameFeel.OnHit(
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

            _gameFeel.OnExplosion(
                projectileId,
                in point,
                radius);
        }
    }
}
