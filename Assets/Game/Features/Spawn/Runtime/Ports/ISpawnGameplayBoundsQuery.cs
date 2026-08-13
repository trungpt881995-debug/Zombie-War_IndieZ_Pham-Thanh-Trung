using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Ports { public interface ISpawnGameplayBoundsQuery { bool Contains(in SpawnPoint point); } }
