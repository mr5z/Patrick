using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Patrick.Commands;
using Patrick.Enums;
using Patrick.Models;
using Patrick.Models.Events;
using Patrick.Models.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class DiscordService : IChatService
    {
        private const Role DefaultRole = Role.Read | Role.Write;

        private readonly IAppConfigProvider configProvider;
        private readonly ICommandParser commandParser;
        private readonly IServiceProvider serviceProvider;
        private readonly IUserService userService;
        private readonly IAudioService audioService;
        private readonly IEventPropagator eventPropagator;

        private readonly DiscordSocketClient socketClient = new DiscordSocketClient();
        private readonly Color preferredColor = new Color(255, 144, 148);

        public DiscordService(
            IAppConfigProvider configProvider,
            ICommandParser commandParser,
            IServiceCollection serviceCollection,
            IUserService userService,
            IAudioService audioService,
            IEventPropagator eventPropagator)
        {
            this.configProvider = configProvider;
            this.commandParser = commandParser;
            this.userService = userService;
            this.audioService = audioService;
            this.eventPropagator = eventPropagator;

            serviceCollection.AddTransient(typeof(CustomCommand));
            serviceProvider = serviceCollection.BuildServiceProvider();
            socketClient.Ready += SocketClient_Ready;
            socketClient.MessageReceived += Client_MessageReceived;
            socketClient.Disconnected += SocketClient_Disconnected;
            socketClient.Log += SocketClient_Log;
        }

        private static void Log(ConsoleColor color, string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
                return;
            Console.ForegroundColor = color;
            Console.WriteLine(message, args);
        }

        private Task SocketClient_Ready()
        {
            Log(ConsoleColor.White, "I'm alive");
            audioService.Configure(socketClient);
            return Task.CompletedTask;
        }

        private Task SocketClient_Log(LogMessage arg)
        {
            var color = arg.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Error => ConsoleColor.DarkRed,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.Green,
                LogSeverity.Verbose => ConsoleColor.White,
                _ => ConsoleColor.White
            };
            Log(color, arg.Message);
            return Task.CompletedTask;
        }

        private async Task SocketClient_Disconnected(Exception arg)
        {
            Log(ConsoleColor.Red, "Disconnected. Printing stacktrace...");
            Log(ConsoleColor.White, arg.StackTrace ?? "<empty>");
            int retryDelay = 5;
            Log(ConsoleColor.Yellow, "Retrying to connect after {0} seconds");
            await Task.Delay(TimeSpan.FromSeconds(retryDelay));
            await Start();
        }

        public async Task Start()
        {
            var token = configProvider.Configuration.Discord!.Token!;
            await socketClient.LoginAsync(TokenType.Bot, token);
            await socketClient.StartAsync();

            //if (KnownChannels != null)
            //{
            //    foreach(var serverId in KnownChannels)
            //    {
            //        var channel = socketClient.GetChannel(serverId);
            //        if (channel != null && channel is SocketTextChannel textChannel)
            //        {
            //            await textChannel.SendMessageAsync("I'm alive!");
            //        }
            //    }
            //}
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == BotId)
                return;

            var message = arg.Content;

            if (arg.MentionedUsers.Any())
            {
                var server = GetServer(arg);
                var channel = new DiscordChannel(arg.Channel, server);
                eventPropagator.ReportUserMentionEvent(
                    new UserMentionEventArgs(arg.Author.Id, arg.MentionedUsers.Select(e => e.Id), channel)
                );
            }

            if (!message.StartsWith(TriggerText))
                return;

            Log(ConsoleColor.White, "{0} -> {1} say, {2}", DateTime.Now, arg.Author.Username, message);

            try
            {
                var cts = new CancellationTokenSource(TypingDuration);
                await arg.Channel.TriggerTypingAsync(new RequestOptions { CancelToken = cts.Token });
            }
            catch (OperationCanceledException) { }

            var command = await ParseCommand(message);

            if (command == null)
            {
                var result = await arg.Channel.SendMessageAsync(RandomDefaultResponse());
                _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(t => result.DeleteAsync());
                return;
            }

            var user = await userService.Find(arg.Author.Id);

            var currentRole = KnownUsers.Contains(arg.Author.Id) ?
                Role.FullAccess : user?.Role ?? DefaultRole;

            if (!currentRole.HasFlag(command.RoleRequirement))
            {
                var result = await arg.Channel.SendMessageAsync(
                    "Sorry, you don't have the required role to perform this command"
                );
                return;
            }

            var currentServer = GetServer(arg);
            var currentChannel = new DiscordChannel(arg.Channel, currentServer);
            var discordUser = new DiscordUser(arg.Author.Id, currentChannel, currentServer)
            {
                Fullname = arg.Author.Username,
                MessageArgument = command.NewArguments,
                Role = currentRole,
                SessionId = arg.Channel.Id,
                MentionedUsers = arg.MentionedUsers.Select(e => new DiscordUser(e.Id, currentChannel, currentServer)
                {
                    Fullname = e.Username,
                    SessionId = arg.Channel.Id
                }).ToList(),
                IsAudible = (arg.Author is SocketGuildUser u &&
                            u.VoiceChannel != null &&
                            u.VoiceState.HasValue &&
                            !u.VoiceState.Value.IsSelfDeafened)
            };

            var response = await command.PerformAction(discordUser);

            await RespondToChannel(arg.Channel, response, command.UseEmbed);
        }

        private static IServer GetServer(SocketMessage message)
        {
            if (message is SocketUserMessage socketUserMessage)
            {
                if (socketUserMessage.Channel is SocketTextChannel textChannel)
                    return new DiscordServer(textChannel.Guild.Name);
                if (socketUserMessage.Channel is SocketVoiceChannel voiceChannel)
                    return new DiscordServer(voiceChannel.Guild.Name);
            }
            return new MockServer();
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id != BotId)
            {
                _ = Task.Run(() => MessageReceived(arg));
            }
            return Task.CompletedTask;
        }

        private async Task RespondToChannel(ISocketMessageChannel channel, CommandResponse response, bool isEmbed)
        {
            const int DiscordBotMessageLimit = 2000;

            if (string.IsNullOrEmpty(response.Message))
            {
                var result = await channel.SendMessageAsync(RandomDefaultResponse());
                _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(t => result.DeleteAsync());
                return;
            }

            var (tag, token) = response.MessageEnclosure! ?? ("", "");

            var paddedMessage = response.MessageEnclosure.HasValue ? $"{tag}{token}{token}\n\n".Length : 0;
            var chunks = ChunksUpto(response.Message, DiscordBotMessageLimit - paddedMessage);

            foreach(var msg in chunks)
            {
                var message =
                    response.MessageEnclosure.HasValue ?
                    $"{token}{tag}\n{msg}\n{token}" : msg;

                try
                {
                    if (isEmbed)
                    {
                        var embed = new EmbedBuilder { Color = preferredColor }
                            .WithTitle($":key: **__{response.CommandName}__**")
                            .WithDescription(message)
                            .Build();
                        var result = await channel.SendMessageAsync(embed: embed);
                    }
                    else
                    {
                        var result = await channel.SendMessageAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    var result = await channel.SendMessageAsync($"```{ex.Message}```");
                }
            }
        }

        private static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        private async Task<BaseCommand?> ParseCommand(string text)
        {
            var command = await commandParser.Parse(text);
            if (command != null && command.GetType() == typeof(CustomCommand))
            {
                var newCommand = serviceProvider.GetService<CustomCommand>()!;
                newCommand.Name = command.Name;
                newCommand.NewArguments = command.NewArguments;
                newCommand.OldArguments = command.OldArguments;
                newCommand.Usage = command.Usage;
                newCommand.Description = command.Description;
                newCommand.UseEmbed = command.UseEmbed;
                return newCommand;
            }
            return command;
        }

        private readonly Random random = new Random();
        private string RandomDefaultResponse()
        {
            var responses = new string[] {
                "...",
                "https://img.pngio.com/patrick-one-tooth-laugh-animated-gif-gifs-gifsoupcom-little-patrick-star-one-tooth-320_240.gif",
                "https://i.kym-cdn.com/entries/icons/original/000/027/642/dumb.jpg"
            };

            return responses[random.Next(responses.Length)];
        }

        private DiscordModel Discord => configProvider.Configuration!.Discord!;
        public ulong[]? KnownChannels => Discord.KnownChannels;
        public ulong[] KnownUsers => Discord.KnownUsers!;
        public ulong BotId => Discord.BotId;
        public string TriggerText => Discord.TriggerText!;
        public Dictionary<string, string> Icons => Discord.Icons!;
        public TimeSpan TypingDuration => TimeSpan.FromSeconds(Discord.TypingDuration);
    }
}
