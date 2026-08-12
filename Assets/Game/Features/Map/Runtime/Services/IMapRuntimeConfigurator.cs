using System.Threading;
using System.Threading.Tasks;
using ZombieWar.Features.Map.Catalog;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Services
{
    public interface IMapRuntimeConfigurator
    {
        void Initialize(IMapCatalog catalog, IMapAssetLoader assetLoader);
        Task ShutdownAsync(CancellationToken cancellationToken);
    }
}
