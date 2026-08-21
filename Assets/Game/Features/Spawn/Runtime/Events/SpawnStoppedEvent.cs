using GeneralCore.Architecture; 
using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Events 
{ 
  public readonly struct SpawnStoppedEvent : IEvent 
  { 
    public SpawnStopReason Reason { get; } 
    public SpawnStoppedEvent(SpawnStopReason reason)=>Reason=reason; 
  } 
}
