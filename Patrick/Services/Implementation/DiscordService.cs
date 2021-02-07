using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Patrick.Commands;
using Patrick.Enums;
using Patrick.Models;
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

        private readonly DiscordSocketClient socketClient = new DiscordSocketClient();
        private readonly Color preferredColor = new Color(255, 144, 148);

        public DiscordService(
            IAppConfigProvider configProvider,
            ICommandParser commandParser,
            IServiceCollection serviceCollection,
            IUserService userService,
            IAudioService audioService)
        {
            this.configProvider = configProvider;
            this.commandParser = commandParser;
            this.userService = userService;
            this.audioService = audioService;

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

        private static string ToAlternatingCase(string words)
        {
            var newWord = new StringBuilder();
            for(var i = 0;i < words.Length; ++i)
            {
                var c = words[i];
                c = i % 2 == 0 ? char.ToUpper(c) : char.ToLower(c);
                newWord.Append(c);
            }
            return newWord.ToString();
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == BotId)
                return;

            var message = arg.Content;

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
                //try
                //{
                //    await AddReactionText("123", result);
                //}
                //catch (Exception ex)
                //{
                //    var msg = ex.Message;
                //}
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

            var currentChannel = new DiscordChannel(arg.Channel);
            var discordUser = new DiscordUser(arg.Author.Id, currentChannel)
            {
                Fullname = arg.Author.Username,
                MessageArgument = command.NewArguments,
                Role = currentRole,
                SessionId = arg.Channel.Id,
                MentionedUsers = arg.MentionedUsers.Select(e => new DiscordUser(e.Id, currentChannel)
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

        private Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id != BotId)
            {
                _ = Task.Run(() => MessageReceived(arg));
            }
            return Task.CompletedTask;
        }

        private static async Task AddReactionText(string reactionText, RestUserMessage message)
        {
            foreach (var c in reactionText)
            {
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                {
                    var react = LetterToEmoji(c);
                    await message.AddReactionAsync(new Emoji(react));
                }
                else if (c >= '0' && c <= '9')
                    await message.AddReactionAsync(new Emoji(NumberToEmoji(c - '0')));
            }
        }

        private static string NumberToEmoji(int number)
        {
            var code = 0x1f100 + number;
            var result = char.ConvertFromUtf32(code).ToString();
            return result;
        }

        private static string LetterToEmoji(char letter)
        {
            var dictionary = new Dictionary<char, string>
            {
                ['A'] = "\u1F1E6",
                ['B'] = "\u1F1E7",
                ['C'] = "\u1F1E8",
                ['D'] = "\u1F1E9",
                ['E'] = "\u1F1EA",
                ['F'] = "\u1F1EB",
                ['G'] = "\u1F1EC",
                ['H'] = "\u1F1EE",
                ['I'] = "\u1F1EF",
                ['J'] = "\u1F1F0",
                ['K'] = "\u1F1F1",
                ['L'] = "\u1F1F2",
                ['M'] = "\u1F1F3",
                ['N'] = "\u1F1F4",
                ['O'] = "\u1F1F5",
                ['P'] = "\u1F1F6",
                ['Q'] = "\u1F1F7",
                ['R'] = "\u1F1F8",
                ['S'] = "\u1F1F9",
                ['T'] = "\u1F1FA",
                ['U'] = "\u1F1FB",
                ['V'] = "\u1F1FC",
                ['W'] = "\u1F1FD",
                ['X'] = "\u1F1FE",
                ['Y'] = "\u1F200",
                ['Z'] = "\u1F201"
            };

            return dictionary[char.ToUpper(letter)];
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
