using System;
using ZombieWar.Features.Feedback.Catalog;
using ZombieWar.Features.Feedback.Controller;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Ports;

namespace ZombieWar.Features.Feedback.Services
{
    public interface IFeedbackRuntime
    {
        bool IsInitialized { get; }
        FeedbackRuntimeMode Mode { get; }
        FeedbackSnapshot Snapshot { get; }

        FeedbackResult Play(in FeedbackRequest request);
        void SetMode(FeedbackRuntimeMode mode);
        void Tick(float deltaTime);
        void CancelAll();
    }

    public interface IFeedbackRuntimeConfigurator
    {
        void Initialize(
            IFeedbackCatalog catalog,
            ICameraFeedbackPort camera,
            IHapticFeedbackPort haptic,
            IScreenFeedbackPort screen,
            IRecoilFeedbackPort recoil);

        void Shutdown();
    }

    public sealed class FeedbackRuntime : IFeedbackRuntime, IFeedbackRuntimeConfigurator
    {
        private readonly FeedbackController _controller;

        public FeedbackRuntime(FeedbackController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public bool IsInitialized => _controller.IsInitialized;
        public FeedbackRuntimeMode Mode => _controller.Mode;
        public FeedbackSnapshot Snapshot => _controller.Snapshot;

        public FeedbackResult Play(in FeedbackRequest request)
        {
            return _controller.Play(in request);
        }

        public void SetMode(FeedbackRuntimeMode mode)
        {
            _controller.SetMode(mode);
        }

        public void Tick(float deltaTime)
        {
            _controller.Tick(deltaTime);
        }

        public void CancelAll()
        {
            _controller.CancelAll();
        }

        public void Initialize(
            IFeedbackCatalog catalog,
            ICameraFeedbackPort camera,
            IHapticFeedbackPort haptic,
            IScreenFeedbackPort screen,
            IRecoilFeedbackPort recoil)
        {
            _controller.Initialize(
                catalog,
                camera,
                haptic,
                screen,
                recoil);
        }

        public void Shutdown()
        {
            _controller.Shutdown();
        }
    }
}
