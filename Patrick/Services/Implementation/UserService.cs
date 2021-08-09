using Patrick.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class UserService : IUserService
    {
        private const string CollectionName = "Users";

        private readonly IRepository repository;

        public UserService(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<bool> AddUser(IUser user)
        {
            var result = await repository.Add(CollectionName, user);
            return !string.IsNullOrEmpty(result);
        }

        public async Task<bool> AddUsers(IEnumerable<IUser> users)
        {
            var result = await repository.AddList(CollectionName, users);
            return !result.Any(string.IsNullOrEmpty);
        }

        public async Task<IUser?> Find(ulong userId, CancellationToken cancellationToken)
        {
            var userList = await GetUsers(cancellationToken);
            return userList.FirstOrDefault(e => e.Id == userId);
        }

        public async Task<bool> RemoveUser(IUser user, CancellationToken cancellationToken)
        {
            var userList = await GetUsers(cancellationToken);
            var newList = userList.ToList();
            return newList.Remove(user) && await AddUsers(newList);
        }

        public async Task<IReadOnlyCollection<IUser>> GetUsers(CancellationToken cancellationToken)
        {
            return await repository.GetList<IUser>(CollectionName, cancellationToken);
        }

        public Task<bool> ClearUsers(CancellationToken _)
        {
            repository.Clear(CollectionName);
            return Task.FromResult(true);
        }
    }
}
