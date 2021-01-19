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
        private readonly IGistGithubService gistGithubService;

        private readonly DiscordSocketClient socketClient = new DiscordSocketClient();
        private readonly Color preferredColor = new Color(255, 144, 148);

        public DiscordService(
            IAppConfigProvider configProvider,
            ICommandParser commandParser,
            IServiceCollection serviceCollection,
            IUserService userService,
            IAudioService audioService,
            IGistGithubService gistGithubService)
        {
            this.configProvider = configProvider;
            this.commandParser = commandParser;
            this.userService = userService;
            this.audioService = audioService;
            this.gistGithubService = gistGithubService;

            serviceCollection.AddTransient(typeof(CustomCommand));
            serviceProvider = serviceCollection.BuildServiceProvider();
            socketClient.MessageReceived += Client_MessageReceived;
        }

        public async Task Start()
        {
            var token = configProvider.Configuration.Discord!.Token!;
            await socketClient.LoginAsync(TokenType.Bot, token);
            await socketClient.StartAsync();
            audioService.Configure(socketClient);

            Console.WriteLine("I'm alive!");
            if (KnownChannels != null)
            {
                foreach(var serverId in KnownChannels)
                {
                    var channel = socketClient.GetChannel(serverId);
                    if (channel != null && channel is SocketTextChannel textChannel)
                    {
                        await textChannel.SendMessageAsync("I'm alive!");
                    }
                }
            }
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == BotId)
                return;

            var message = arg.Content;

            if (!message.StartsWith(TriggerText))
                return;

            Console.WriteLine("{0} -> {1} say, {2}", DateTime.Now, arg.Author.Username, arg.Content);

            try
            {
                //await gistGithubService.Authenticate();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

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
                //await AddReactionText("Deleting in 5 seconds", result);
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
                }).ToList()
            };

            var response = await command.PerformAction(discordUser);

            await RespondToChannel(arg.Channel, response, command.UseEmbed);
        }

        private static async Task AddReactionText(string reactionText, RestUserMessage message)
        {
            //await message.AddReactionAsync(Emote.Parse("<:regional_indicator_a:bbe8ae762f831966587a35010ed46f67>"));
            //return;
            foreach (var c in reactionText)
            {
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z')
                {
                    var react = $"<:regional_indicator_{char.ToLower(c)}:>";
                    Emote? emote = null;
                    try
                    {
                        emote = Emote.Parse(react);
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                        continue;
                    }
                    await message.AddReactionAsync(Emote.Parse(react));
                }
                else if (c >= '0' && c <= '9')
                    await message.AddReactionAsync(Emote.Parse($":{NumberToWord(c - '0')}:"));
                else if (c == ' ')
                    await message.AddReactionAsync(Emote.Parse(":heavy_minus_sign:"));
            }
        }

        private static string NumberToWord(int number)
        {
            var dictionary = new Dictionary<int, string>
            {
                [0] = "zero",
                [1] = "one",
                [2] = "two",
                [3] = "three",
                [4] = "four",
                [5] = "five",
                [6] = "six",
                [7] = "seven",
                [8] = "eigth",
                [9] = "nine"
            };
            return dictionary[number];
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
        public ulong[]? KnownUsers => Discord.KnownUsers;
        public ulong BotId => Discord.BotId;
        public string TriggerText => Discord.TriggerText!;
        public Dictionary<string, string> Icons => Discord.Icons!;
        public TimeSpan TypingDuration => TimeSpan.FromSeconds(Discord.TypingDuration);
    }
}
