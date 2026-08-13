using System;

namespace ZombieWar.Features.Feedback.Domain
{
    [Flags]
    public enum FeedbackChannel
    {
        None = 0,
        Camera = 1 << 0,
        Haptic = 1 << 1,
        Screen = 1 << 2,
        Recoil = 1 << 3
    }
}
