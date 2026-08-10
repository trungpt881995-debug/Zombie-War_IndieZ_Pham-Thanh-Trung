using System.Threading;
using System.Threading.Tasks;

namespace GeneralCore.Scenes
{
    public interface ISceneService
    {
        Task LoadAsync(string sceneName, CancellationToken cancellationToken = default);
        Task UnloadAsync(string sceneName, CancellationToken cancellationToken = default);
    }
}
