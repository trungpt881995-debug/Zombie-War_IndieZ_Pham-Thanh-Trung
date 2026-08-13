using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Integration.Feedback.Weapon;

namespace ZombieWar.Integration.Audio.Weapon
{
    public sealed class WeaponAudioFeedbackPort :
        IWeaponFeedbackPort
    {
        private readonly IAudioRuntime _audio;
        private readonly IWeaponMuzzleProvider _muzzles;

        private readonly Dictionary<long, AudioHandle> _flameHandles =
            new Dictionary<long, AudioHandle>(4);

        public WeaponAudioFeedbackPort(
            IAudioRuntime audio,
            IWeaponMuzzleProvider muzzles)
        {
            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));

            _muzzles = muzzles ??
                throw new ArgumentNullException(nameof(muzzles));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            AudioId id = Map(weapon);

            if (id == AudioId.None ||
                !_muzzles.TryGetMuzzle(
                    ownerId,
                    out WeaponMuzzle muzzle))
            {
                return;
            }

            var point =
                new AudioPoint(
                    muzzle.Position.X,
                    muzzle.Position.Y,
                    muzzle.Position.Z);

            var request =
                new AudioRequest(
                    id,
                    in point,
                    1f,
                    ownerId.Value);

            _audio.Play(in request);
        }

        public void OnFlameStarted(
            EntityId ownerId)
        {
            if (_flameHandles.TryGetValue(
                    ownerId.Value,
                    out AudioHandle existing))
            {
                if (_audio.IsPlaying(existing))
                {
                    return;
                }

                _flameHandles.Remove(ownerId.Value);
            }

            var anchor =
                new WeaponMuzzleAudioAnchor(
                    ownerId,
                    _muzzles);

            if (!anchor.IsValid)
            {
                return;
            }

            var request =
                new AudioRequest(
                    AudioId.FlamethrowerLoop,
                    anchor,
                    1f,
                    ownerId.Value);

            AudioPlayResult result =
                _audio.Play(in request);

            if (result.Accepted &&
                result.Handle.IsValid)
            {
                _flameHandles[ownerId.Value] =
                    result.Handle;
            }
        }

        public void OnFlameStopped(
            EntityId ownerId)
        {
            if (!_flameHandles.TryGetValue(
                    ownerId.Value,
                    out AudioHandle handle))
            {
                return;
            }

            _flameHandles.Remove(ownerId.Value);
            _audio.Stop(handle);
        }

        private static AudioId Map(
            WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Pistol:
                    return AudioId.PistolFire;

                case WeaponType.AK:
                    return AudioId.AKFire;

                case WeaponType.Shotgun:
                    return AudioId.ShotgunFire;

                case WeaponType.SniperRifle:
                    return AudioId.SniperFire;

                case WeaponType.GrenadeLauncher:
                    return AudioId.GrenadeFire;

                default:
                    return AudioId.None;
            }
        }
    }

    public sealed class CompositeWeaponPresentationFeedbackPort :
        IWeaponFeedbackPort
    {
        private readonly CompositeWeaponFeedbackPort _existing;
        private readonly WeaponAudioFeedbackPort _audio;

        public CompositeWeaponPresentationFeedbackPort(
            CompositeWeaponFeedbackPort existing,
            WeaponAudioFeedbackPort audio)
        {
            _existing = existing ??
                throw new ArgumentNullException(nameof(existing));

            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            _existing.OnShotFired(
                ownerId,
                weapon);

            _audio.OnShotFired(
                ownerId,
                weapon);
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            _existing.OnFlameStarted(ownerId);
            _audio.OnFlameStarted(ownerId);
        }

        public void OnFlameStopped(EntityId ownerId)
        {
            _existing.OnFlameStopped(ownerId);
            _audio.OnFlameStopped(ownerId);
        }
    }

    internal sealed class WeaponMuzzleAudioAnchor :
        IAudioAnchor
    {
        private readonly EntityId _ownerId;
        private readonly IWeaponMuzzleProvider _muzzles;

        public WeaponMuzzleAudioAnchor(
            EntityId ownerId,
            IWeaponMuzzleProvider muzzles)
        {
            _ownerId = ownerId;
            _muzzles = muzzles;
        }

        public bool IsValid =>
            _muzzles.TryGetMuzzle(
                _ownerId,
                out _);

        public AudioPoint Position
        {
            get
            {
                if (!_muzzles.TryGetMuzzle(
                        _ownerId,
                        out WeaponMuzzle muzzle))
                {
                    return default;
                }

                return new AudioPoint(
                    muzzle.Position.X,
                    muzzle.Position.Y,
                    muzzle.Position.Z);
            }
        }
    }
}
