using Patrick.Models;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface IAfkStore
    {
        Task<AfkResponse?> FindStoreMessageFor(ulong userId);
        Task<bool> StoreMessageFor(ulong userId, string? authorName, string message);
        Task<bool> ClearMessageFor(ulong userId);
        Task<bool> ClearAllMessages();
    }
}
