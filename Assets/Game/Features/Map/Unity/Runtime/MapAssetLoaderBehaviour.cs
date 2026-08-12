using UnityEngine;
using ZombieWar.Features.Map.Ports;

namespace ZombieWar.Features.Map.Unity.Runtime
{
    public abstract class MapAssetLoaderBehaviour : MonoBehaviour, IMapAssetLoader
    {
        protected Transform Parent { get; private set; }

        public void SetParent(Transform parent)
        {
            Parent = parent;
        }

        public abstract System.Threading.Tasks.Task<IMapInstance> LoadAsync(
            ZombieWar.Features.Map.Domain.MapDefinition definition,
            System.Threading.CancellationToken cancellationToken);

        public abstract System.Threading.Tasks.Task ReleaseAsync(
            IMapInstance instance,
            System.Threading.CancellationToken cancellationToken);
    }
}
