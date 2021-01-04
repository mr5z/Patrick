using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class DiscordService : IDiscordService
    {
        private readonly DiscordSocketClient client = new DiscordSocketClient();
        private readonly IAppConfigProvider configProvider;
        private ICommandParser commandParser;

        public DiscordService(IAppConfigProvider configProvider, ICommandParser commandParser)
        {
            this.configProvider = configProvider;
            this.commandParser = commandParser;
            client.MessageReceived += Client_MessageReceived;
        }

        public async Task Start()
        {
            var token = configProvider.Configuration.Discord!.Token!;
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            var message = arg.Content;

            Console.WriteLine(arg.Content);

            if (!message.StartsWith(TriggerText))
                return;

            var command = await commandParser.Parse(message);
            if (command != null)
            {
                var messageResponse = await command.PerformAction(command.Arguments);
                Console.WriteLine(messageResponse);
                var result = arg.Channel.SendMessageAsync(messageResponse);
                var a = result;
            }
        }

        public string TriggerText => configProvider.Configuration!.Discord!.TriggerText!;
    }
}
