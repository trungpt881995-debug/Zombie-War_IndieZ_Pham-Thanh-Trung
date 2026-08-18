using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.VFX.Commands;
namespace ZombieWar.Bootstrap
{
    public sealed class VFXCommandRegistration:IStartable
    {
        private readonly ICommandRegistry _r; 
        private readonly PlayVFXCommandHandler _play; 
        private readonly StopVFXCommandHandler _stop; 
        private readonly SetVFXModeCommandHandler _mode; 
        private readonly CancelAllVFXCommandHandler _cancel;
        public VFXCommandRegistration(ICommandRegistry r,PlayVFXCommandHandler play,StopVFXCommandHandler stop,SetVFXModeCommandHandler mode,CancelAllVFXCommandHandler cancel)
        {
            _r=r;
            _play=play;
            _stop=stop;
            _mode=mode;
            _cancel=cancel;
        }
        public void Start()
        {
            _r.Register<PlayVFXCommand>(_play);
            _r.Register<StopVFXCommand>(_stop);
            _r.Register<SetVFXModeCommand>(_mode);
            _r.Register<CancelAllVFXCommand>(_cancel);
        }
    }
}
