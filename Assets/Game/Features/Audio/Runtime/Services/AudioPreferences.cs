using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Services
{
    public interface IAudioPreferences
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SFXVolume { get; }
        float UIVolume { get; }
        bool Muted { get; }

        float GetCategoryVolume(AudioCategory category);
    }

    public sealed class AudioPreferences : IAudioPreferences
    {
        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private float _uiVolume = 1f;

        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Clamp01(value);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set => _musicVolume = Clamp01(value);
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Clamp01(value);
        }

        public float UIVolume
        {
            get => _uiVolume;
            set => _uiVolume = Clamp01(value);
        }

        public bool Muted { get; set; }

        public float GetCategoryVolume(AudioCategory category)
        {
            if (Muted)
            {
                return 0f;
            }

            float categoryVolume;

            switch (category)
            {
                case AudioCategory.Music:
                    categoryVolume = MusicVolume;
                    break;

                case AudioCategory.UI:
                    categoryVolume = UIVolume;
                    break;

                default:
                    categoryVolume = SFXVolume;
                    break;
            }

            return MasterVolume * categoryVolume;
        }

        private static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return value >= 1f ? 1f : value;
        }
    }
}
