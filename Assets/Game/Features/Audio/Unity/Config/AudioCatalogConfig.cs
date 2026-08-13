using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieWar.Features.Audio.Catalog;
using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Unity.Config
{
    [CreateAssetMenu(
        fileName = "AudioCatalog_Game",
        menuName = "Zombie War/Audio/Audio Catalog")]
    public sealed class AudioCatalogConfig : ScriptableObject
    {
        [SerializeField] private AudioConfig[] entries = new AudioConfig[0];

        public AudioConfig[] Entries => entries;

        public IAudioCatalog BuildCatalog()
        {
            var definitions =
                new List<AudioDefinition>(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                AudioConfig entry = entries[i];

                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"Audio catalog has a null entry at index {i}.");
                }

                definitions.Add(entry.BuildDefinition());
            }

            return new AudioCatalog(definitions);
        }

        public bool TryGetConfig(
            AudioId id,
            out AudioConfig config)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                AudioConfig candidate = entries[i];

                if (candidate != null &&
                    candidate.Id == id)
                {
                    config = candidate;
                    return true;
                }
            }

            config = null;
            return false;
        }

#if UNITY_EDITOR
        public void ConfigureForEditor(AudioConfig[] newEntries)
        {
            entries = newEntries ?? new AudioConfig[0];
        }
#endif
    }
}
