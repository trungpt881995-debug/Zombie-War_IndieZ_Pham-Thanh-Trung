using System; 
using GeneralCore.Architecture; 
using ZombieWar.Features.Spawn.Services;
namespace ZombieWar.Features.Spawn.Commands 
{ 
  public sealed class StartZombieSpawningCommandHandler:ICommandHandler<StartZombieSpawningCommand>
  {
    private readonly ISpawnRuntime _runtime; 
    public StartZombieSpawningCommandHandler(ISpawnRuntime runtime)=> _runtime = runtime??throw new ArgumentNullException(nameof(runtime)); 
    public void Handle(StartZombieSpawningCommand command)=>_runtime.Start();
  } 
}
