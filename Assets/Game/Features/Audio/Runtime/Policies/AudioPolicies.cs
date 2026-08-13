using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Policies
{
    public interface IAudioConcurrencyPolicy
    {
        bool CanPlay(
            in AudioDefinition definition,
            int currentCount);
    }

    public sealed class AudioConcurrencyPolicy : IAudioConcurrencyPolicy
    {
        public bool CanPlay(
            in AudioDefinition definition,
            int currentCount)
        {
            return currentCount < definition.MaxConcurrent;
        }
    }

    public interface IAudioModePolicy
    {
        bool CanPlay(
            WorldAudioMode mode,
            in AudioDefinition definition);
    }

    public sealed class AudioModePolicy : IAudioModePolicy
    {
        public bool CanPlay(
            WorldAudioMode mode,
            in AudioDefinition definition)
        {
            if (definition.Category == AudioCategory.Music)
            {
                return false;
            }

            if (definition.Category == AudioCategory.UI)
            {
                return true;
            }

            switch (mode)
            {
                case WorldAudioMode.Playing:
                    return true;

                case WorldAudioMode.TerminalDrain:
                    return definition.AllowDuringTerminalDrain;

                default:
                    return false;
            }
        }
    }
}
