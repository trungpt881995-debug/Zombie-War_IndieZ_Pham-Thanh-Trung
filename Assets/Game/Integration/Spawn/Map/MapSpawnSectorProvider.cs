using System; using ZombieWar.Features.Map.Domain; using ZombieWar.Features.Map.Services; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Map
{
    public sealed class MapSpawnSectorProvider : ISpawnSectorProvider
    {
        private readonly IMapRuntime _map; public MapSpawnSectorProvider(IMapRuntime map)=>_map=map??throw new ArgumentNullException(nameof(map));
        public bool TryGetSector(SpawnSectorId id,out SpawnSector sector)
        {
            sector=default;if(!_map.TryGetCurrentContext(out MapRuntimeContext context))return false;
            if(!context.TryGetSpawnSector((MapSpawnSectorId)(int)id,out MapSpawnSector source))return false;
            MapArea a=source.Area;var area=new SpawnArea(a.MinX,a.MaxX,a.MinZ,a.MaxZ);sector=new SpawnSector(id,in area);return true;
        }
    }
}
