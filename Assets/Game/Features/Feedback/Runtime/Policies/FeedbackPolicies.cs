using System.Collections.Generic;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Policies
{
    public interface IHapticCooldownPolicy
    {
        bool TryConsume(
            FeedbackId id,
            float now,
            float cooldown);

        void Reset();
    }

    public sealed class HapticCooldownPolicy : IHapticCooldownPolicy
    {
        private readonly Dictionary<FeedbackId, float> _nextAllowed =
            new Dictionary<FeedbackId, float>(16);

        public bool TryConsume(
            FeedbackId id,
            float now,
            float cooldown)
        {
            if (cooldown <= 0f)
            {
                return true;
            }

            if (_nextAllowed.TryGetValue(id, out float nextAllowed) &&
                now < nextAllowed)
            {
                return false;
            }

            _nextAllowed[id] = now + cooldown;
            return true;
        }

        public void Reset()
        {
            _nextAllowed.Clear();
        }
    }

    public interface IFeedbackPriorityPolicy
    {
        bool TryAcquire(
            FeedbackChannel channel,
            FeedbackPriority priority,
            float now,
            float occupancyDuration);

        void Reset();
    }

    public sealed class FeedbackPriorityPolicy : IFeedbackPriorityPolicy
    {
        private readonly Dictionary<FeedbackChannel, Occupancy> _occupancy =
            new Dictionary<FeedbackChannel, Occupancy>(3);

        public bool TryAcquire(
            FeedbackChannel channel,
            FeedbackPriority priority,
            float now,
            float occupancyDuration)
        {
            if (occupancyDuration <= 0f)
            {
                return true;
            }

            if (_occupancy.TryGetValue(channel, out Occupancy current) &&
                now < current.Until &&
                priority < current.Priority)
            {
                return false;
            }

            _occupancy[channel] = new Occupancy(
                priority,
                now + occupancyDuration);

            return true;
        }

        public void Reset()
        {
            _occupancy.Clear();
        }

        private readonly struct Occupancy
        {
            public FeedbackPriority Priority { get; }
            public float Until { get; }

            public Occupancy(
                FeedbackPriority priority,
                float until)
            {
                Priority = priority;
                Until = until;
            }
        }
    }
}
