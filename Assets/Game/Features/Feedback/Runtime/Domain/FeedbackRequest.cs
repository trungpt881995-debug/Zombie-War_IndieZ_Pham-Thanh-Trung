using System;

namespace ZombieWar.Features.Feedback.Domain
{
    public readonly struct FeedbackRequest
    {
        public FeedbackId Id { get; }
        public float Intensity { get; }
        public long SourceId { get; }

        public FeedbackRequest(
            FeedbackId id,
            float intensity = 1f,
            long sourceId = 0L)
        {
            if (id == FeedbackId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (float.IsNaN(intensity) ||
                float.IsInfinity(intensity) ||
                intensity <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity));
            }

            Id = id;
            Intensity = intensity;
            SourceId = sourceId;
        }
    }
}
