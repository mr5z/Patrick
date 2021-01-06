using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Models
{
    interface IChannel
    {
        Task<IReadOnlyCollection<IChannelMessage>> GetMessages(int count, CancellationToken cancellationToken = default);
        Task<bool> DeleteMessage(ulong messageId, CancellationToken cancellationToken = default);
    }
}
