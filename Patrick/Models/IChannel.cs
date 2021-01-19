using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Models
{
    interface IChannel
    {
        ulong Id { get; }
        Task<bool> SendMessage(CommandResponse response);
        Task<IReadOnlyCollection<IUser>> GetActiveUsers(CancellationToken cancellationToken = default);
        Task<IUser?> FindUser(ulong userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<IChannelMessage>> GetMessages(int count, CancellationToken cancellationToken = default);
        Task<bool> DeleteMessage(ulong messageId, CancellationToken cancellationToken = default);
    }
}
