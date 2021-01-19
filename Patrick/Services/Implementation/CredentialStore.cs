using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class CredentialStore : ICredentialStore
    {
        private const string CollectionName = "Credentials";

        private readonly IRepository repository;

        public CredentialStore(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<string?> LoadAccessToken()
        {
            var accessTokens = await repository.GetList<string?>(CollectionName);
            return accessTokens.FirstOrDefault();
        }

        public async Task<bool> StoreAccessToken(string accessToken)
        {
            var id = await repository.Add(CollectionName, accessToken);
            return !string.IsNullOrEmpty(id);
        }
    }
}
