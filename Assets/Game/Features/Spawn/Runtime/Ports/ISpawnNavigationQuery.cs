using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Ports { public interface ISpawnNavigationQuery { bool TryResolve(in SpawnPoint candidate,out SpawnPoint resolved); } }
