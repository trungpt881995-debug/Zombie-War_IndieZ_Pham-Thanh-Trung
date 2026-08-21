using System; 
using ZombieWar.Features.Spawn.Domain; 
using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Features.Spawn.Strategies
{
    public sealed class RandomSpawnPositionStrategy : ISpawnPositionStrategy
    {
        public SpawnPoint Select(in SpawnArea area, ISpawnRandom random)
        {
            if(!area.IsValid) 
            throw new ArgumentException("Spawn area must be valid.",nameof(area));

            if(random==null) 
            throw new ArgumentNullException(nameof(random));

            float x=area.MinX+(area.MaxX-area.MinX)*random.Value();
            float z=area.MinZ+(area.MaxZ-area.MinZ)*random.Value();
            return new SpawnPoint(x,0f,z);
        }
    }
}
