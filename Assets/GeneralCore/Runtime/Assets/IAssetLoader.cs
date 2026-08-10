using System.Threading;
using System.Threading.Tasks;

namespace GeneralCore.Assets
{
    public interface IAssetHandle<out T> where T : class
    {
        T Asset { get; }
    }

    public interface IAssetLoader
    {
        Task<IAssetHandle<T>> LoadAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        void Release<T>(IAssetHandle<T> handle) where T : class;
    }
}
