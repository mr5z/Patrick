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
                    RetryMode = RetryMode.RetryRatelimit,
                    CancelToken = cancellationToken
                });
                return true;
            }
            catch (OperationCanceledException) { return false; }
            catch (Discord.Net.HttpException) { return false; }
        }

        public async Task<IUser?> FindUser(ulong userId, CancellationToken cancellationToken = default)
        {
            var activeUsers = await GetActiveUsers(cancellationToken);
            return activeUsers.FirstOrDefault(e => e.Id == userId);
        }

        public async Task<IReadOnlyCollection<IUser>> GetActiveUsers(CancellationToken cancellationToken)
        {
            var users = await channel.GetUsersAsync(mode: CacheMode.AllowDownload, options: new RequestOptions
            {
                CancelToken = cancellationToken
            }).FlattenAsync();

            return new List<IUser>(users.Select(e => new DiscordUser(e.Id, this)
            {
                Fullname = e.Username,
                CurrentChannel = this,
                SessionId = channel.Id,
                Status = e.Status != UserStatus.Offline ? Enums.UserStatus.Online : Enums.UserStatus.Offline
            }));
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
