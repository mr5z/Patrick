using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Models.Implementation
{
    class MockChannel : IChannel
    {
        public ulong Id { get; }

        public bool IsAudible => false;

        public string Name => "Mock Channel";

        public Task<bool> DeleteMessage(ulong messageId, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public async Task<IUser?> FindUser(ulong userId, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new DiscordUser(userId, this, new MockServer()));
        }

        public async Task<IReadOnlyCollection<IUser>> GetActiveUsers(CancellationToken cancellationToken)
        {
            var result = Enumerable.Empty<IUser>();
            return await Task.FromResult(result.ToList());
        }

        public async Task<IReadOnlyCollection<IChannelMessage>> GetMessages(int count, CancellationToken cancellationToken)
        {
            var result = Enumerable.Empty<IChannelMessage>();
            return await Task.FromResult(result.ToList());
        }

        public Task<bool> SendMessage(CommandResponse response)
        {
            return Task.FromResult(true);
        }
    }
}
