using Patrick.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface IUserService
    {
        Task<IReadOnlyCollection<IUser>> GetUsers(CancellationToken cancellationToken = default);
        Task<bool> AddUser(IUser user);
        Task<bool> AddUsers(IEnumerable<IUser> users);
        Task<IUser?> Find(ulong userId, CancellationToken cancellationToken = default);
        Task<bool> RemoveUser(IUser user, CancellationToken cancellationToken = default);
    }
}
