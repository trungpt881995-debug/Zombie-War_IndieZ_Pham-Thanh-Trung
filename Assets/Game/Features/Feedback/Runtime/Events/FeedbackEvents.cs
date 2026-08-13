using GeneralCore.Architecture;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Events
{
    public readonly struct FeedbackPlayedEvent : IEvent
    {
        public FeedbackId Id { get; }
        public FeedbackChannel Channels { get; }
        public long Sequence { get; }

        public FeedbackPlayedEvent(
            FeedbackId id,
            FeedbackChannel channels,
            long sequence)
        {
            Id = id;
            Channels = channels;
            Sequence = sequence;
        }
    }

    public readonly struct FeedbackRejectedEvent : IEvent
    {
        public FeedbackId Id { get; }
        public FeedbackFailure Failure { get; }

        public FeedbackRejectedEvent(
            FeedbackId id,
            FeedbackFailure failure)
        {
            Id = id;
            Failure = failure;
        }
    }
}
