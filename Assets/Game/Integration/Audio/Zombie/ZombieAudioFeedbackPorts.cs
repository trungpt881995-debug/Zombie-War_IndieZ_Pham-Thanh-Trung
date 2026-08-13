using System;
using GameplayCore.Entities;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;
using ZombieWar.Integration.VFX.Zombie;

namespace ZombieWar.Integration.Audio.Zombie
{
    public sealed class ZombieAudioFeedbackPort :
        IZombieFeedbackPort
    {
        private readonly IAudioRuntime _audio;

        public ZombieAudioFeedbackPort(
            IAudioRuntime audio)
        {
            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));
        }

        public void OnHit(
            EntityId zombieId,
            in ZombiePoint position)
        {
            Play(
                AudioId.ZombieHit,
                zombieId,
                in position);
        }

        public void OnDeath(
            EntityId zombieId,
            in ZombiePoint position)
        {
            Play(
                AudioId.ZombieDeath,
                zombieId,
                in position);
        }

        private void Play(
            AudioId id,
            EntityId entityId,
            in ZombiePoint position)
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

    public sealed class CompositeZombiePresentationFeedbackPort :
        IZombieFeedbackPort
    {
        private readonly ZombieVFXFeedbackPort _vfx;
        private readonly ZombieAudioFeedbackPort _audio;

        public CompositeZombiePresentationFeedbackPort(
            ZombieVFXFeedbackPort vfx,
            ZombieAudioFeedbackPort audio)
        {
            _vfx = vfx ??
                throw new ArgumentNullException(nameof(vfx));

            _audio = audio ??
                throw new ArgumentNullException(nameof(audio));
        }

        public void OnHit(
            EntityId zombieId,
            in ZombiePoint position)
        {
            _vfx.OnHit(
                zombieId,
                in position);

            _audio.OnHit(
                zombieId,
                in position);
        }

        public void OnDeath(
            EntityId zombieId,
            in ZombiePoint position)
        {
            _vfx.OnDeath(
                zombieId,
                in position);

            _audio.OnDeath(
                zombieId,
                in position);
        }
    }
}
