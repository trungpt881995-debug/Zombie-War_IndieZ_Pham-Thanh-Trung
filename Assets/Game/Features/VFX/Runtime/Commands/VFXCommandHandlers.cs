using System; using GeneralCore.Architecture; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Features.VFX.Commands
{
    public sealed class PlayVFXCommandHandler:ICommandHandler<PlayVFXCommand>{private readonly IVFXRuntime _r;public PlayVFXCommandHandler(IVFXRuntime r){_r=r??throw new ArgumentNullException(nameof(r));}public void Handle(PlayVFXCommand c){var q=c.Request;_r.Play(in q);}}
    public sealed class StopVFXCommandHandler:ICommandHandler<StopVFXCommand>{private readonly IVFXRuntime _r;public StopVFXCommandHandler(IVFXRuntime r){_r=r;}public void Handle(StopVFXCommand c)=>_r.Stop(c.Handle);}
    public sealed class SetVFXModeCommandHandler:ICommandHandler<SetVFXModeCommand>{private readonly IVFXRuntime _r;public SetVFXModeCommandHandler(IVFXRuntime r){_r=r;}public void Handle(SetVFXModeCommand c)=>_r.SetMode(c.Mode);}
    public sealed class CancelAllVFXCommandHandler:ICommandHandler<CancelAllVFXCommand>{private readonly IVFXRuntime _r;public CancelAllVFXCommandHandler(IVFXRuntime r){_r=r;}public void Handle(CancelAllVFXCommand c)=>_r.CancelAll();}
}
