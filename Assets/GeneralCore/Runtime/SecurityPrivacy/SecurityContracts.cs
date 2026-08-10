using System.Threading;
using System.Threading.Tasks;

namespace GeneralCore.SecurityPrivacy
{
    public interface ISecretStore
    {
        Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
        Task<string> GetAsync(string key, CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    }

    public interface IPrivacyConsentService
    {
        bool HasConsent(string purpose);
    }
}
