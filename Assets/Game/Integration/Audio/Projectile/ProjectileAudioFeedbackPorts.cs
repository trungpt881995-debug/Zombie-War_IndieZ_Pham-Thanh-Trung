using System;
using GameplayCore.Entities;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Ports;

namespace ZombieWar.Integration.Audio.Projectile
{
    public sealed class ProjectileAudioFeedbackPort : IProjectileFeedbackPort
    {
        private readonly IAudioRuntime _audio;

        public ProjectileAudioFeedbackPort(IAudioRuntime audio)
        {
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public void OnHit(
            EntityId projectileId,
            EntityId targetId,
            in ProjectilePoint point)
        {
            AudioPoint audioPoint = Convert(in point);

            var request = new AudioRequest(
                AudioId.BulletImpact,
                in audioPoint,
                1f,
                projectileId.Value);

            _audio.Play(in request);
        }

        public void OnExplosion(
            EntityId projectileId,
            in ProjectilePoint point,
            float radius)
        {
            AudioPoint audioPoint = Convert(in point);

            float intensity =
                radius <= 0f
                    ? 1f
                    : Math.Min(
                        1.5f,
                        Math.Max(
                            0.75f,
                            radius / 4f));

            var request = new AudioRequest(
                AudioId.GrenadeExplosion,
                in audioPoint,
                intensity,
                projectileId.Value);

            _audio.Play(in request);
        }

        private static AudioPoint Convert(in ProjectilePoint point)
        {
            return new AudioPoint(
                point.X,
                point.Y,
                point.Z);
        }
    }
}
