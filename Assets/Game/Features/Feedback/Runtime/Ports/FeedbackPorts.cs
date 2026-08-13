using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Ports
{
    public interface ICameraFeedbackPort
    {
        bool TryPlay(FeedbackCameraCue cue);
        void CancelAll();
    }

    public interface IHapticFeedbackPort
    {
        bool TryPlay(HapticFeedbackStrength strength);
        void CancelAll();
    }

    public interface IScreenFeedbackPort
    {
        bool TryFlash(
            ScreenFeedbackKind kind,
            float intensity,
            float duration);

        void SetSuspended(bool suspended);
        void Clear();
    }

    public interface IRecoilFeedbackPort
    {
        bool TryApply(
            float strength,
            float duration);

        void CancelAll();
    }

    public sealed class NullRecoilFeedbackPort : IRecoilFeedbackPort
    {
        public static readonly NullRecoilFeedbackPort Instance =
            new NullRecoilFeedbackPort();

        private NullRecoilFeedbackPort()
        {
        }

        public bool TryApply(
            float strength,
            float duration)
        {
            return false;
        }

        public void CancelAll()
        {
        }
    }
}
