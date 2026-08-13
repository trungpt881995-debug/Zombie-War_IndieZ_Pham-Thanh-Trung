using System; using ZombieWar.Features.Map.Domain; using ZombieWar.Features.Map.Services; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Map
{
    public sealed class MapGameplayBoundsQuery : ISpawnGameplayBoundsQuery
    {
        private readonly IMapRuntime _map; public MapGameplayBoundsQuery(IMapRuntime map)=>_map=map??throw new ArgumentNullException(nameof(map));
        public bool Contains(in SpawnPoint point)
        {
            if(!_map.TryGetCurrentContext(out MapRuntimeContext context))return false;
            var p=new MapPoint(point.X,point.Y,point.Z);return context.GameplayBounds.Contains(in p);
        }
    }
}
