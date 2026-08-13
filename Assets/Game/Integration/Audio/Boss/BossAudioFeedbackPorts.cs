using System;
using GameplayCore.Entities;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Integration.Feedback.Boss;

namespace ZombieWar.Integration.Audio.Boss
{
    public sealed class BossAudioFeedbackPort :
        IBossFeedbackPort
    {
        private readonly IAudioRuntime _audio;

        public BossAudioFeedbackPort(
            IAudioRuntime audio)
        {
            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));
        }

        public void OnSpawn(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            Play(
                AudioId.BossSpawn,
                entityId,
                in position);
        }

        public void OnHit(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            Play(
                AudioId.BossHit,
                entityId,
                in position);
        }

        public void OnDeath(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            Play(
                AudioId.BossDeath,
                entityId,
                in position);
        }

        private void Play(
            AudioId id,
            EntityId entityId,
            in BossPoint position)
        {
            var point =
                new AudioPoint(
                    position.X,
                    position.Y,
                    position.Z);

            var request =
                new AudioRequest(
                    id,
                    in point,
                    1f,
                    entityId.Value);

            _audio.Play(in request);
        }
    }

    public sealed class CompositeBossPresentationFeedbackPort :
        IBossFeedbackPort
    {
        private readonly CompositeBossFeedbackPort _existing;
        private readonly BossAudioFeedbackPort _audio;

        public CompositeBossPresentationFeedbackPort(
            CompositeBossFeedbackPort existing,
            BossAudioFeedbackPort audio)
        {
            _existing = existing ??
                throw new ArgumentNullException(nameof(existing));

            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));
        }

        public void OnSpawn(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _existing.OnSpawn(
                bossId,
                entityId,
                in position);

            _audio.OnSpawn(
                bossId,
                entityId,
                in position);
        }

        public void OnHit(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _existing.OnHit(
                bossId,
                entityId,
                in position);

            _audio.OnHit(
                bossId,
                entityId,
                in position);
        }

        public void OnDeath(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _existing.OnDeath(
                bossId,
                entityId,
                in position);

            _audio.OnDeath(
                bossId,
                entityId,
                in position);
        }
    }
}
