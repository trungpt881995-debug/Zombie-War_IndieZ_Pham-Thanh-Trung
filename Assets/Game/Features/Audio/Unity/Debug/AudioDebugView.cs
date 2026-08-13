using UnityEngine;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Unity.Runtime;

namespace ZombieWar.Features.Audio.Unity.Debugging
{
    public sealed class AudioDebugView :
        MonoBehaviour
    {
        [SerializeField]
        private AudioRuntimeRoot runtimeRoot;

        [ContextMenu("Audio/Mode Playing")]
        public void ModePlaying()
        {
            Runtime?.SetWorldMode(
                WorldAudioMode.Playing);
        }

        [ContextMenu("Audio/Mode Suspended")]
        public void ModeSuspended()
        {
            Runtime?.SetWorldMode(
                WorldAudioMode.Suspended);
        }

        [ContextMenu("Audio/Play Pistol 2D Debug")]
        public void PlayPistol()
        {
            Play(AudioId.PistolFire);
        }

        [ContextMenu("Audio/Play Boss Death 2D Debug")]
        public void PlayBossDeath()
        {
            Play(AudioId.BossDeath);
        }

        [ContextMenu("Audio/Main Menu Music")]
        public void MainMenuMusic()
        {
            Runtime?.PlayMusic(
                AudioId.MainMenuMusic);
        }

        [ContextMenu("Audio/Gameplay Music")]
        public void GameplayMusic()
        {
            Runtime?.PlayMusic(
                AudioId.GameplayMusic);
        }

        [ContextMenu("Audio/Stop Music")]
        public void StopMusic()
        {
            Runtime?.StopMusic();
        }

        [ContextMenu("Audio/Cancel All")]
        public void CancelAll()
        {
            Runtime?.CancelAll();
        }

        private ZombieWar.Features.Audio.Services.IAudioRuntime Runtime =>
            runtimeRoot != null
                ? runtimeRoot.Runtime
                : null;

        private void Play(AudioId id)
        {
            if (Runtime == null)
            {
                return;
            }

            var point =
                new AudioPoint(0f, 0f, 0f);

            var request =
                new AudioRequest(
                    id,
                    in point);

            Runtime.Play(in request);
        }
    }
}
