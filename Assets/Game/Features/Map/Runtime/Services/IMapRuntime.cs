using System.Threading;
using System.Threading.Tasks;
using ZombieWar.Features.Map.Domain;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Services
{
    public interface IMapRuntime : IMapContextProvider
    {
        bool IsInitialized { get; }
        MapState State { get; }
        MapId CurrentMapId { get; }
        MapRuntimeContext CurrentContext { get; }

        Task<MapLoadResult> LoadAsync(MapId mapId, CancellationToken cancellationToken);
        Task UnloadAsync(CancellationToken cancellationToken);
    }
}
