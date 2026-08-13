namespace ZombieWar.Features.Feedback.Domain
{
    public enum FeedbackFailure
    {
        None = 0,
        NotInitialized = 1,
        RuntimeModeRejected = 2,
        DefinitionNotFound = 3,
        TerminalRejected = 4
    }

    public readonly struct FeedbackResult
    {
        public bool Accepted { get; }
        public FeedbackId Id { get; }
        public FeedbackFailure Failure { get; }
        public FeedbackChannel ExecutedChannels { get; }
        public long Sequence { get; }

        private FeedbackResult(
            bool accepted,
            FeedbackId id,
            FeedbackFailure failure,
            FeedbackChannel executedChannels,
            long sequence)
        {
            Accepted = accepted;
            Id = id;
            Failure = failure;
            ExecutedChannels = executedChannels;
            Sequence = sequence;
        }

        public static FeedbackResult Accept(
            FeedbackId id,
            FeedbackChannel channels,
            long sequence)
        {
            return new FeedbackResult(
                true,
                id,
                FeedbackFailure.None,
                channels,
                sequence);
        }

        public static FeedbackResult Reject(
            FeedbackId id,
            FeedbackFailure failure,
            long sequence)
        {
            return new FeedbackResult(
                false,
                id,
                failure,
                FeedbackChannel.None,
                sequence);
        }
    }

    public readonly struct FeedbackSnapshot
    {
        public bool IsInitialized { get; }
        public FeedbackRuntimeMode Mode { get; }
        public float Elapsed { get; }
        public long Sequence { get; }
        public int AcceptedCount { get; }
        public int RejectedCount { get; }

        public FeedbackSnapshot(
            bool isInitialized,
            FeedbackRuntimeMode mode,
            float elapsed,
            long sequence,
            int acceptedCount,
            int rejectedCount)
        {
            IsInitialized = isInitialized;
            Mode = mode;
            Elapsed = elapsed;
            Sequence = sequence;
            AcceptedCount = acceptedCount;
            RejectedCount = rejectedCount;
        }
    }
}
