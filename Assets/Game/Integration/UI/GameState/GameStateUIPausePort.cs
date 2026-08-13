using System; using GeneralCore.Architecture; using ZombieWar.Features.GameState.Commands; using ZombieWar.Features.UI.Ports;
namespace ZombieWar.Integration.UI.GameState
{
    public sealed class GameStateUIPausePort:IGameplayPausePort
    { private readonly ICommandBus _commands; public GameStateUIPausePort(ICommandBus commands)=>_commands=commands??throw new ArgumentNullException(nameof(commands)); public void Pause()=>_commands.Send(new PauseGameplayCommand()); public void Resume()=>_commands.Send(new ResumeGameplayCommand()); }
}
