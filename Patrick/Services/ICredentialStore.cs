using System.Threading.Tasks;

namespace Patrick.Services
{
    interface ICredentialStore
    {
        Task<bool> StoreAccessToken(string accessToken);
        Task<string?> LoadAccessToken();
    }
}
