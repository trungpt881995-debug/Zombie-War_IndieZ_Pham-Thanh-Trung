using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Audio.Commands;

namespace ZombieWar.Bootstrap
{
    public sealed class AudioCommandRegistration : IStartable
    {
        private readonly ICommandRegistry _registry;
        private readonly PlayAudioCommandHandler _play;
        private readonly StopAudioCommandHandler _stop;
        private readonly SetWorldAudioModeCommandHandler _mode;
        private readonly PlayMusicCommandHandler _playMusic;
        private readonly StopMusicCommandHandler _stopMusic;
        private readonly CancelAllAudioCommandHandler _cancel;

        public AudioCommandRegistration(
            ICommandRegistry registry,
            PlayAudioCommandHandler play,
            StopAudioCommandHandler stop,
            SetWorldAudioModeCommandHandler mode,
            PlayMusicCommandHandler playMusic,
            StopMusicCommandHandler stopMusic,
            CancelAllAudioCommandHandler cancel)
        {
            _registry = registry;
            _play = play;
            _stop = stop;
            _mode = mode;
            _playMusic = playMusic;
            _stopMusic = stopMusic;
            _cancel = cancel;
        }

        public void Start()
        {
            _registry.Register<PlayAudioCommand>(_play);
            _registry.Register<StopAudioCommand>(_stop);
            _registry.Register<SetWorldAudioModeCommand>(_mode);
            _registry.Register<PlayMusicCommand>(_playMusic);
            _registry.Register<StopMusicCommand>(_stopMusic);
            _registry.Register<CancelAllAudioCommand>(_cancel);
        }
    }
}
