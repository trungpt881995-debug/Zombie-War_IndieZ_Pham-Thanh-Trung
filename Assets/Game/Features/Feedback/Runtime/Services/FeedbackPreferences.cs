namespace ZombieWar.Features.Feedback.Services
{
    public interface IFeedbackPreferences
    {
        bool CameraEnabled { get; }
        bool HapticEnabled { get; }
        bool ScreenEnabled { get; }
        bool RecoilEnabled { get; }
    }

    public sealed class FeedbackPreferences : IFeedbackPreferences
    {
        public bool CameraEnabled { get; set; } = true;
        public bool HapticEnabled { get; set; } = true;
        public bool ScreenEnabled { get; set; } = true;
        public bool RecoilEnabled { get; set; } = true;
    }
}
