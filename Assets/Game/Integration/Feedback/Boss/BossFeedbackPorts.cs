using System;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Integration.VFX.Boss;

namespace ZombieWar.Integration.Feedback.Boss
{
    public sealed class BossGameFeelFeedbackPort : IBossFeedbackPort
    {
        private readonly IFeedbackRuntime _feedback;

        public BossGameFeelFeedbackPort(IFeedbackRuntime feedback)
        {
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        public void OnSpawn(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
        }

        public void OnHit(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            Play(
                FeedbackId.BossHit,
                entityId.Value);
        }

        public void OnDeath(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            Play(
                FeedbackId.BossDefeated,
                entityId.Value);
        }

        private void Play(
            FeedbackId id,
            long sourceId)
        {
            var request = new FeedbackRequest(
                id,
                1f,
                sourceId);

            _feedback.Play(in request);
        }
    }

    public sealed class CompositeBossFeedbackPort : IBossFeedbackPort
    {
        private readonly BossVFXFeedbackPort _vfx;
        private readonly BossGameFeelFeedbackPort _gameFeel;

        public CompositeBossFeedbackPort(
            BossVFXFeedbackPort vfx,
            BossGameFeelFeedbackPort gameFeel)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _gameFeel = gameFeel ?? throw new ArgumentNullException(nameof(gameFeel));
        }

        public void OnSpawn(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _vfx.OnSpawn(
                bossId,
                entityId,
                in position);

            _gameFeel.OnSpawn(
                bossId,
                entityId,
                in position);
        }

        public void OnHit(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _vfx.OnHit(
                bossId,
                entityId,
                in position);

            _gameFeel.OnHit(
                bossId,
                entityId,
                in position);
        }

        public void OnDeath(
            BossId bossId,
            EntityId entityId,
            in BossPoint position)
        {
            _vfx.OnDeath(
                bossId,
                entityId,
                in position);

            _gameFeel.OnDeath(
                bossId,
                entityId,
                in position);
        }
    }
}
