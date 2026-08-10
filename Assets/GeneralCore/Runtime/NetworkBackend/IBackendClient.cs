using System.Threading;
using System.Threading.Tasks;

namespace GeneralCore.NetworkBackend
{
    public interface IBackendClient
    {
        Task<string> GetAsync(string route, CancellationToken cancellationToken = default);
        Task<string> PostAsync(string route, string payload, CancellationToken cancellationToken = default);
    }
}
