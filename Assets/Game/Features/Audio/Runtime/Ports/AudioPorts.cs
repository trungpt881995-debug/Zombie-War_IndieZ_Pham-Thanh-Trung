using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Ports
{
    public interface IAudioAnchor
    {
        bool IsValid { get; }
        AudioPoint Position { get; }
    }

    public interface IAudioVoiceLease
    {
        bool IsPlaying { get; }
        bool IsPaused { get; }

        bool TryPlay(
            in AudioDefinition definition,
            in AudioRequest request,
            float volume,
            float pitch);

        void SetPaused(bool paused);
        void SetVolume(float volume);
        void SetPosition(in AudioPoint position);
        void Stop();
        void Release();
    }

    public interface IAudioVoicePool
    {
        int Capacity { get; }
        int AvailableCount { get; }

        bool TryAcquire(out IAudioVoiceLease lease);
    }

    public interface IMusicPlaybackPort
    {
        AudioId CurrentMusic { get; }

        bool Play(
            AudioId id,
            float fadeDuration,
            float volume);

        void Stop(float fadeDuration);
        void SetVolume(float volume);
        void Tick(float deltaTime);
        void Clear();
    }

    public interface IAudioRandom
    {
        float Range(
            float minInclusive,
            float maxInclusive);
    }
}
