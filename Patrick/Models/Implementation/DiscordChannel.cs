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

        public ulong Id { get; }

        public bool IsAudible => false;

        public DiscordChannel(ISocketMessageChannel channel)
        {
            this.channel = channel;
            Id = channel?.Id ?? 0;
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
            var users = await channel.GetUsersAsync(options: new RequestOptions
            {
                CancelToken = cancellationToken
            }).FlattenAsync();

            return new List<IUser>(users.Select(e => new DiscordUser(e.Id, this)
            {
                Fullname = e.Username,
                CurrentChannel = this,
                SessionId = channel.Id,
                Status = e.Status == UserStatus.Offline ? Enums.UserStatus.Offline : Enums.UserStatus.Online
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

        public async Task<bool> SendMessage(CommandResponse response)
        {
            if (response.UseEmbed)
            {
                var embed = new EmbedBuilder { }
                    .WithTitle($":key: **__{response.CommandName}__**")
                    .WithDescription(response.Message)
                    .Build();
                var result = await channel.SendMessageAsync(embed: embed);
            }
            else
            {
                var result = await channel.SendMessageAsync(response.Message);
            }

            return true;
        }
    }
}
