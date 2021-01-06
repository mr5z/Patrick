using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Models.Implementation
{
    class DiscordChannel : IChannel
    {
        private readonly ISocketMessageChannel channel;

        public DiscordChannel(ISocketMessageChannel channel)
        {
            this.channel = channel;
        }

        public async Task<bool> DeleteMessage(ulong messageId, CancellationToken cancellationToken)
        {
            try
            {
                await channel.DeleteMessageAsync(messageId, new RequestOptions
                {
                    CancelToken = cancellationToken
                });
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Discord.Net.HttpException) { return false; }
        }

        public async Task<IReadOnlyCollection<IChannelMessage>> GetMessages(int count, CancellationToken cancellationToken)
        {
            var messages = await channel.GetMessagesAsync(count, options: new RequestOptions
            {
                CancelToken = cancellationToken
            }).FlattenAsync();
            return new List<IChannelMessage>(messages.Select(e => new DiscordChannelMessage(e.Id)));
        }
    }
}
