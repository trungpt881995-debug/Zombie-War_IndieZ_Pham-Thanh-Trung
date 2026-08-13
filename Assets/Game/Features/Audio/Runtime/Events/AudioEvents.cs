using GeneralCore.Architecture;
using ZombieWar.Features.Audio.Domain;

namespace ZombieWar.Features.Audio.Events
{
    public readonly struct AudioPlayedEvent : IEvent
    {
        public AudioId Id { get; }
        public long SourceId { get; }
        public AudioHandle Handle { get; }
        public long Sequence { get; }

        public AudioPlayedEvent(
            AudioId id,
            long sourceId,
            AudioHandle handle,
            long sequence)
        {
            Id = id;
            SourceId = sourceId;
            Handle = handle;
            Sequence = sequence;
        }
    }

    public readonly struct AudioRejectedEvent : IEvent
    {
        public AudioId Id { get; }
        public AudioFailure Failure { get; }

        public AudioRejectedEvent(
            AudioId id,
            AudioFailure failure)
        {
            Id = id;
            Failure = failure;
        }
    }

    public readonly struct AudioReleasedEvent : IEvent
    {
        public AudioId Id { get; }
        public AudioReleaseReason Reason { get; }

        public AudioReleasedEvent(
            AudioId id,
            AudioReleaseReason reason)
        {
            Id = id;
            Reason = reason;
        }
    }
}
