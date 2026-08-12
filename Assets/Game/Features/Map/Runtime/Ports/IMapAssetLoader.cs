using System.Threading;
using System.Threading.Tasks;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Ports
{
    public interface IMapAssetLoader
    {
        Task<IMapInstance> LoadAsync(MapDefinition definition, CancellationToken cancellationToken);
        Task ReleaseAsync(IMapInstance instance, CancellationToken cancellationToken);
    }
}
