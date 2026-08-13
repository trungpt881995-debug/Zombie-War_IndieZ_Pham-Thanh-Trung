using System; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Features.Spawn.Strategies
{
    public sealed class RandomSpawnSectorSelectionStrategy : ISpawnSectorSelectionStrategy
    {
        public SpawnSectorId Select(ISpawnRandom random) { if(random==null) throw new ArgumentNullException(nameof(random)); return (SpawnSectorId)random.Range(0,4); }
    }
}
