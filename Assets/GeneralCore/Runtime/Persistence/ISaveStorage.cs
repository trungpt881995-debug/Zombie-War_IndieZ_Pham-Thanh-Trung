using System.Threading;
using System.Threading.Tasks;

namespace GeneralCore.Persistence
{
    public interface ISaveStorage
    {
        Task SaveAsync(string key, string payload, CancellationToken cancellationToken = default);
        Task<string> LoadAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    }
}
