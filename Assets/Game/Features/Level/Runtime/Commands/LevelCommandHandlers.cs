using System; using GeneralCore.Architecture; using ZombieWar.Features.Level.Services;
namespace ZombieWar.Features.Level.Commands
{
    public sealed class BeginGameLevelCommandHandler:ICommandHandler<BeginGameLevelCommand>{private readonly ILevelRuntime _r;public BeginGameLevelCommandHandler(ILevelRuntime r){_r=r??throw new ArgumentNullException(nameof(r));}public void Handle(BeginGameLevelCommand c){_r.BeginLevel(c.GameLevel);}}
    public sealed class RegisterNormalZombieKillCommandHandler:ICommandHandler<RegisterNormalZombieKillCommand>{private readonly ILevelRuntime _r;public RegisterNormalZombieKillCommandHandler(ILevelRuntime r){_r=r??throw new ArgumentNullException(nameof(r));}public void Handle(RegisterNormalZombieKillCommand c){_r.RegisterNormalZombieKill();}}
    public sealed class RegisterBossDefeatedCommandHandler:ICommandHandler<RegisterBossDefeatedCommand>{private readonly ILevelRuntime _r;public RegisterBossDefeatedCommandHandler(ILevelRuntime r){_r=r??throw new ArgumentNullException(nameof(r));}public void Handle(RegisterBossDefeatedCommand c){_r.RegisterBossDefeated(c.Boss);}}
    public sealed class SetLevelProgressionEnabledCommandHandler:ICommandHandler<SetLevelProgressionEnabledCommand>{private readonly ILevelRuntime _r;public SetLevelProgressionEnabledCommandHandler(ILevelRuntime r){_r=r??throw new ArgumentNullException(nameof(r));}public void Handle(SetLevelProgressionEnabledCommand c){_r.SetProgressionEnabled(c.Enabled);}}
}
