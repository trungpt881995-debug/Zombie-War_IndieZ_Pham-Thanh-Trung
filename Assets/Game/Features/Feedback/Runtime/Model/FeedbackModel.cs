using System;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Model
{
    public sealed class FeedbackModel
    {
        public bool IsInitialized { get; private set; }
        public FeedbackRuntimeMode Mode { get; private set; } = FeedbackRuntimeMode.Inactive;
        public float Elapsed { get; private set; }
        public long Sequence { get; private set; }
        public int AcceptedCount { get; private set; }
        public int RejectedCount { get; private set; }

        public void Initialize()
        {
            IsInitialized = true;
            Elapsed = 0f;
            Sequence = 0L;
            AcceptedCount = 0;
            RejectedCount = 0;
        }

        public void Shutdown()
        {
            IsInitialized = false;
            Mode = FeedbackRuntimeMode.Inactive;
            Elapsed = 0f;
        }

        public void SetMode(FeedbackRuntimeMode mode)
        {
            Mode = mode;
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (Mode == FeedbackRuntimeMode.Playing ||
                Mode == FeedbackRuntimeMode.TerminalDrain)
            {
                Elapsed += deltaTime;
            }
        }

        public long RecordAccepted()
        {
            AcceptedCount++;
            Sequence++;
            return Sequence;
        }

        public void RecordRejected()
        {
            RejectedCount++;
        }
    }
}
