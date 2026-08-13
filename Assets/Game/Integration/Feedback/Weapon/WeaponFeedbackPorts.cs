using System;
using GameplayCore.Entities;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Integration.VFX.Weapon;

namespace ZombieWar.Integration.Feedback.Weapon
{
    public sealed class WeaponGameFeelFeedbackPort : IWeaponFeedbackPort
    {
        private readonly IFeedbackRuntime _feedback;

        public WeaponGameFeelFeedbackPort(IFeedbackRuntime feedback)
        {
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            FeedbackId id = Map(weapon);

            if (id == FeedbackId.None)
            {
                return;
            }

            var request = new FeedbackRequest(
                id,
                1f,
                ownerId.Value);

            _feedback.Play(in request);
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            var request = new FeedbackRequest(
                FeedbackId.FlamethrowerStart,
                1f,
                ownerId.Value);

            _feedback.Play(in request);
        }

        public void OnFlameStopped(EntityId ownerId)
        {
        }

        private static FeedbackId Map(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Pistol:
                    return FeedbackId.PistolShot;

                case WeaponType.AK:
                    return FeedbackId.AKShot;

                case WeaponType.Shotgun:
                    return FeedbackId.ShotgunShot;

                case WeaponType.SniperRifle:
                    return FeedbackId.SniperShot;

                case WeaponType.GrenadeLauncher:
                    return FeedbackId.GrenadeShot;

                default:
                    return FeedbackId.None;
            }
        }
    }

    public sealed class CompositeWeaponFeedbackPort : IWeaponFeedbackPort
    {
        private readonly WeaponVFXFeedbackPort _vfx;
        private readonly WeaponGameFeelFeedbackPort _gameFeel;

        public CompositeWeaponFeedbackPort(
            WeaponVFXFeedbackPort vfx,
            WeaponGameFeelFeedbackPort gameFeel)
        {
            _vfx = vfx ?? throw new ArgumentNullException(nameof(vfx));
            _gameFeel = gameFeel ?? throw new ArgumentNullException(nameof(gameFeel));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            _vfx.OnShotFired(
                ownerId,
                weapon);

            _gameFeel.OnShotFired(
                ownerId,
                weapon);
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            _vfx.OnFlameStarted(ownerId);
            _gameFeel.OnFlameStarted(ownerId);
        }

        public void OnFlameStopped(EntityId ownerId)
        {
            _vfx.OnFlameStopped(ownerId);
            _gameFeel.OnFlameStopped(ownerId);
        }
    }
}
