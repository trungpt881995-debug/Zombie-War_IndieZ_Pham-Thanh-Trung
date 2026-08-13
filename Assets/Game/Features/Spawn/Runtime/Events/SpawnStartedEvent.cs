using GeneralCore.Architecture; using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Events { public readonly struct SpawnStartedEvent : IEvent { public SpawnDifficultyKey Difficulty { get; } public SpawnStartedEvent(in SpawnDifficultyKey difficulty)=>Difficulty=difficulty; } }
