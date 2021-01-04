using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Patrick.Commands;
using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class DiscordService : IChatService
    {
        private readonly DiscordSocketClient client = new DiscordSocketClient();
        private readonly IAppConfigProvider configProvider;
        private readonly ICommandParser commandParser;
        private readonly IServiceProvider serviceProvider;
        private readonly Color preferredColor = new Color(255, 144, 148);

        public DiscordService(
            IAppConfigProvider configProvider,
            ICommandParser commandParser,
            IServiceCollection serviceCollection)
        {
            this.configProvider = configProvider;
            this.commandParser = commandParser;
            serviceCollection.AddTransient(typeof(CustomCommand));
            serviceProvider = serviceCollection.BuildServiceProvider();
            client.MessageReceived += Client_MessageReceived;

        }

        public async Task Start()
        {
            var token = configProvider.Configuration.Discord!.Token!;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            if (KnownChannels != null)
            {
                foreach(var serverId in KnownChannels)
                {
                    if (client.GetChannel(serverId) is IMessageChannel channel)
                    {
                        await channel.SendMessageAsync("I'm alive!");
                    }
                }
            }
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.Id == BotId)
                return;

            var message = arg.Content;

            Console.WriteLine(arg.Content);

            if (!message.StartsWith(TriggerText))
                return;

            var cts = new CancellationTokenSource(TypingDuration);
            await arg.Channel.TriggerTypingAsync(new RequestOptions { CancelToken = cts.Token });
            var command = await ParseCommand(message);
            if (command != null)
            {
                var response = await command.PerformAction(new User
                {
                    Fullname = arg.Author.Username,
                    MessageArgument = command.NewArguments
                });
                Console.WriteLine(response.Message);

                if (command.UseEmbed)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = preferredColor,
                        Description = response.Message
                    }
                    .WithTitle(response.CommandName)
                    .Build();
                    var result = await arg.Channel.SendMessageAsync(embed: embed);
                }
                else
                {
                    var result = await arg.Channel.SendMessageAsync(response.Message);
                }
            }
            else
            {
                var result = await arg.Channel.SendMessageAsync("...");
            }
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

        private DiscordModel Discord => configProvider.Configuration!.Discord!;
        public ulong[]? KnownChannels => Discord.KnownChannels;
        public ulong BotId => Discord.BotId;
        public string TriggerText => Discord.TriggerText!;
        public TimeSpan TypingDuration => TimeSpan.FromSeconds(Discord.TypingDuration);
    }
}
