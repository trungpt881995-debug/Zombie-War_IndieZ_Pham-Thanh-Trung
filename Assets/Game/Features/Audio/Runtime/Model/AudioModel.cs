using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Model
{
    public sealed class AudioModel
    {
        public WorldAudioMode WorldMode { get; private set; } = WorldAudioMode.Inactive;
        public int ActiveVoiceCount { get; private set; }
        public long PlayedCount { get; private set; }
        public long RejectedCount { get; private set; }
        public long ReleasedCount { get; private set; }
        public long Sequence { get; private set; }

        internal long RegisterPlayed()
        {
            PlayedCount++;
            Sequence++;
            return Sequence;
        }

        internal void RegisterRejected()
        {
            RejectedCount++;
        }

        internal void RegisterReleased()
        {
            ReleasedCount++;
        }

        internal void SetActiveVoiceCount(int value)
        {
            ActiveVoiceCount = value < 0 ? 0 : value;
        }

        internal void SetWorldMode(WorldAudioMode mode)
        {
            WorldMode = mode;
        }

        internal void ResetRuntimeState()
        {
            ActiveVoiceCount = 0;
            WorldMode = WorldAudioMode.Inactive;
        }
    }
}
