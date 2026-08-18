using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Feedback.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class FeedbackCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _registry;
        private readonly PlayFeedbackCommandHandler _play;
        private readonly SetFeedbackModeCommandHandler _mode;
        private readonly CancelFeedbackCommandHandler _cancel;

        public FeedbackCommandRegistration(ICommandRegistry registry, PlayFeedbackCommandHandler play, SetFeedbackModeCommandHandler mode, CancelFeedbackCommandHandler cancel)
        {
            _registry = registry;
            _play = play;
            _mode = mode;
            _cancel = cancel;
        }

        public void Start()
        {
            _registry.Register<PlayFeedbackCommand>(_play);
            _registry.Register<SetFeedbackModeCommand>(_mode);
            _registry.Register<CancelFeedbackCommand>(_cancel);
        }
    }
}
