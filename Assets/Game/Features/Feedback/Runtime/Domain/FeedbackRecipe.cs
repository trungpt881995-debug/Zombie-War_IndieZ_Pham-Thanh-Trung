using System;

namespace ZombieWar.Features.Feedback.Domain
{
    public readonly struct FeedbackRecipe
    {
        public FeedbackId Id { get; }
        public FeedbackPriority Priority { get; }
        public bool AllowDuringTerminalDrain { get; }
        public CameraFeedbackDefinition Camera { get; }
        public HapticFeedbackDefinition Haptic { get; }
        public ScreenFeedbackDefinition Screen { get; }
        public RecoilFeedbackDefinition Recoil { get; }

        public FeedbackRecipe(
            FeedbackId id,
            FeedbackPriority priority,
            bool allowDuringTerminalDrain,
            in CameraFeedbackDefinition camera,
            in HapticFeedbackDefinition haptic,
            in ScreenFeedbackDefinition screen,
            in RecoilFeedbackDefinition recoil)
        {
            if (id == FeedbackId.None)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Id = id;
            Priority = priority;
            AllowDuringTerminalDrain = allowDuringTerminalDrain;
            Camera = camera;
            Haptic = haptic;
            Screen = screen;
            Recoil = recoil;
        }
    }
}
