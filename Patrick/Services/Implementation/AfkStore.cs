using Patrick.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class AfkStore : IAfkStore
    {
        private const string CollectionName = "AfkStore";

        private readonly IRepository repository;

        public AfkStore(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<bool> ClearMessageFor(ulong userId)
        {
            var store = await repository.GetList<AfkResponse>(CollectionName);
            store.RemoveWhere(e => e.AuthorId == userId);
            var result = await repository.AddList(CollectionName, store);
            return result.Any();
        }

        public Task<bool> ClearAllMessages()
        {
            repository.Clear(CollectionName);
            return Task.FromResult(true);
        }

        public async Task<AfkResponse?> FindStoreMessageFor(ulong userId)
        {
            var store = await repository.GetList<AfkResponse>(CollectionName);
            return store.FirstOrDefault(e => e.AuthorId == userId);
        }

        public async Task<bool> StoreMessageFor(ulong authorId, string? authorName, string message)
        {
            var result = await repository.Add(CollectionName, new AfkResponse
            {
                AuthorId = authorId,
                AuthorName = authorName,
                Message = message
            });
            return !string.IsNullOrEmpty(result);
        }
    }
}
