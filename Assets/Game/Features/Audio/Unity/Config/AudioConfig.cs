using UnityEngine;
using UnityEngine.Audio;
using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Unity.Config
{
    [CreateAssetMenu(
        fileName = "AudioConfig",
        menuName = "Zombie War/Audio/Audio Config")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioId id = AudioId.PistolFire;
        [SerializeField] private AudioClip[] clips = new AudioClip[0];
        [SerializeField] private AudioCategory category = AudioCategory.SFX;
        [SerializeField] private AudioLifetimeMode lifetimeMode = AudioLifetimeMode.OneShot;
        [SerializeField] private AudioSpatialMode spatialMode = AudioSpatialMode.ThreeD;
        [SerializeField] private AudioPriority priority = AudioPriority.Normal;
        [SerializeField, Min(1)] private int maxConcurrent = 4;
        [SerializeField, Min(0f)] private float baseVolume = 1f;
        [SerializeField, Min(0.01f)] private float minPitch = 0.98f;
        [SerializeField, Min(0.01f)] private float maxPitch = 1.02f;
        [SerializeField, Min(0f)] private float minDistance = 1f;
        [SerializeField, Min(0f)] private float maxDistance = 25f;
        [SerializeField] private bool allowDuringTerminalDrain;
        [SerializeField] private AudioMixerGroup outputMixerGroup;

        public AudioId Id => id;
        public AudioClip[] Clips => clips;
        public AudioMixerGroup OutputMixerGroup => outputMixerGroup;

        public AudioDefinition BuildDefinition()
        {
            return new AudioDefinition(
                id,
                category,
                lifetimeMode,
                spatialMode,
                priority,
                maxConcurrent,
                baseVolume,
                minPitch,
                maxPitch,
                minDistance,
                maxDistance,
                allowDuringTerminalDrain);
        }

#if UNITY_EDITOR
        public void ConfigureForEditor(
            AudioId newId,
            AudioClip[] newClips,
            AudioCategory newCategory,
            AudioLifetimeMode newLifetimeMode,
            AudioSpatialMode newSpatialMode,
            AudioPriority newPriority,
            int newMaxConcurrent,
            float newBaseVolume,
            float newMinPitch,
            float newMaxPitch,
            float newMinDistance,
            float newMaxDistance,
            bool newAllowDuringTerminalDrain)
        {
            id = newId;
            clips = newClips ?? new AudioClip[0];
            category = newCategory;
            lifetimeMode = newLifetimeMode;
            spatialMode = newSpatialMode;
            priority = newPriority;
            maxConcurrent = newMaxConcurrent;
            baseVolume = newBaseVolume;
            minPitch = newMinPitch;
            maxPitch = newMaxPitch;
            minDistance = newMinDistance;
            maxDistance = newMaxDistance;
            allowDuringTerminalDrain = newAllowDuringTerminalDrain;
        }
#endif
    }
}
