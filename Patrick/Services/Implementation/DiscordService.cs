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
        private readonly ICommandStore commandStore;
        private readonly IAppConfigProvider configProvider;

        public DiscordService(ICommandStore commandStore, IAppConfigProvider configProvider)
        {
            this.commandStore = commandStore;
            this.configProvider = configProvider;
            client.MessageReceived += Client_MessageReceived;
        }

        public async Task Start()
        {
            var token = configProvider.Configuration.Discord!.Token!;
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            var message = arg.Content;

            if (message.StartsWith(TriggerText))
            {
                message = message.Substring(0, TriggerText.Length);
            }

            return Task.CompletedTask;
        }

        public string TriggerText => configProvider.Configuration!.Discord!.TriggerText!;
    }
}
