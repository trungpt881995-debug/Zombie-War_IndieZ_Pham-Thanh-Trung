using System;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;

namespace ZombieWar.Integration.Audio.UI
{
    public interface IUIAudioPort
    {
        void PlayButtonClick();
        void PlayWeaponSelected();
    }

    public sealed class UIAudioPort : IUIAudioPort
    {
        private readonly IAudioRuntime _audio;

        public UIAudioPort(IAudioRuntime audio)
        {
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        public void PlayButtonClick()
        {
            Play(AudioId.UIButtonClick);
        }

        public void PlayWeaponSelected()
        {
            Play(AudioId.WeaponSelected);
        }

        private void Play(AudioId id)
        {
            var request = new AudioRequest(id);
            _audio.Play(in request);
        }
    }
}
