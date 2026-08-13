using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;

namespace ZombieWar.Features.Feedback.Commands
{
    public readonly struct PlayFeedbackCommand : ICommand
    {
        public FeedbackRequest Request { get; }

        public PlayFeedbackCommand(in FeedbackRequest request)
        {
            Request = request;
        }
    }

    public readonly struct SetFeedbackModeCommand : ICommand
    {
        public FeedbackRuntimeMode Mode { get; }

        public SetFeedbackModeCommand(FeedbackRuntimeMode mode)
        {
            Mode = mode;
        }
    }

    public readonly struct CancelFeedbackCommand : ICommand
    {
    }

    public sealed class PlayFeedbackCommandHandler : ICommandHandler<PlayFeedbackCommand>
    {
        private readonly IFeedbackRuntime _runtime;

        public PlayFeedbackCommandHandler(IFeedbackRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(PlayFeedbackCommand command)
        {
            FeedbackRequest request = command.Request;
            _runtime.Play(in request);
        }
    }

    public sealed class SetFeedbackModeCommandHandler : ICommandHandler<SetFeedbackModeCommand>
    {
        private readonly IFeedbackRuntime _runtime;

        public SetFeedbackModeCommandHandler(IFeedbackRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(SetFeedbackModeCommand command)
        {
            _runtime.SetMode(command.Mode);
        }
    }

    public sealed class CancelFeedbackCommandHandler : ICommandHandler<CancelFeedbackCommand>
    {
        private readonly IFeedbackRuntime _runtime;

        public CancelFeedbackCommandHandler(IFeedbackRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(CancelFeedbackCommand command)
        {
            _runtime.CancelAll();
        }
    }
}
