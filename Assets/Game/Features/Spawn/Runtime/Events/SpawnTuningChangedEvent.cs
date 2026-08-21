using GeneralCore.Architecture; 
using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Events 
{ 
  public readonly struct SpawnTuningChangedEvent : IEvent 
  { 
    public SpawnDifficultyKey Difficulty { get; } 
    public SpawnTuning Tuning { get; } 
    public SpawnTuningChangedEvent(in SpawnDifficultyKey difficulty,in SpawnTuning tuning)
    {
      Difficulty=difficulty;
      Tuning=tuning;
    } 
  } 
}
