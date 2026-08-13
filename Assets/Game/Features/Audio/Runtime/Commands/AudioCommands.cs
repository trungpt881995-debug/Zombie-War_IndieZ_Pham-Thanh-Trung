using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Audio.Domain;
using ZombieWar.Features.Audio.Services;

namespace ZombieWar.Features.Audio.Commands
{
    public readonly struct PlayAudioCommand : ICommand
    {
        public AudioRequest Request { get; }

        public PlayAudioCommand(in AudioRequest request)
        {
            Request = request;
        }
    }

    public readonly struct StopAudioCommand : ICommand
    {
        public AudioHandle Handle { get; }

        public StopAudioCommand(AudioHandle handle)
        {
            Handle = handle;
        }
    }

    public readonly struct SetWorldAudioModeCommand : ICommand
    {
        public WorldAudioMode Mode { get; }

        public SetWorldAudioModeCommand(WorldAudioMode mode)
        {
            Mode = mode;
        }
    }

    public readonly struct PlayMusicCommand : ICommand
    {
        public AudioId Id { get; }
        public float FadeDuration { get; }

        public PlayMusicCommand(
            AudioId id,
            float fadeDuration = 0.35f)
        {
            Id = id;
            FadeDuration = fadeDuration;
        }
    }

    public readonly struct StopMusicCommand : ICommand
    {
        public float FadeDuration { get; }

        public StopMusicCommand(float fadeDuration = 0.35f)
        {
            FadeDuration = fadeDuration;
        }
    }

    public readonly struct CancelAllAudioCommand : ICommand
    {
    }

    public sealed class PlayAudioCommandHandler :
        ICommandHandler<PlayAudioCommand>
    {
        private readonly IAudioRuntime _runtime;

        public PlayAudioCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(PlayAudioCommand command)
        {
            AudioRequest request = command.Request;
            _runtime.Play(in request);
        }
    }

    public sealed class StopAudioCommandHandler :
        ICommandHandler<StopAudioCommand>
    {
        private readonly IAudioRuntime _runtime;

        public StopAudioCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(StopAudioCommand command)
        {
            _runtime.Stop(command.Handle);
        }
    }

    public sealed class SetWorldAudioModeCommandHandler :
        ICommandHandler<SetWorldAudioModeCommand>
    {
        private readonly IAudioRuntime _runtime;

        public SetWorldAudioModeCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(SetWorldAudioModeCommand command)
        {
            _runtime.SetWorldMode(command.Mode);
        }
    }

    public sealed class PlayMusicCommandHandler :
        ICommandHandler<PlayMusicCommand>
    {
        private readonly IAudioRuntime _runtime;

        public PlayMusicCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(PlayMusicCommand command)
        {
            _runtime.PlayMusic(
                command.Id,
                command.FadeDuration);
        }
    }

    public sealed class StopMusicCommandHandler :
        ICommandHandler<StopMusicCommand>
    {
        private readonly IAudioRuntime _runtime;

        public StopMusicCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(StopMusicCommand command)
        {
            _runtime.StopMusic(command.FadeDuration);
        }
    }

    public sealed class CancelAllAudioCommandHandler :
        ICommandHandler<CancelAllAudioCommand>
    {
        private readonly IAudioRuntime _runtime;

        public CancelAllAudioCommandHandler(IAudioRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(CancelAllAudioCommand command)
        {
            _runtime.CancelAll();
        }
    }
}
