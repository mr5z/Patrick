using Patrick.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class CommandParser : ICommandParser
    {
        private readonly IAppConfigProvider configProvider;
        private readonly ICommandStore commandStore;

        public CommandParser(IAppConfigProvider configProvider, ICommandStore commandStore)
        {
            this.configProvider = configProvider;
            this.commandStore = commandStore;
        }

        public async Task<BaseCommand?> Parse(string text, bool hasTriggerText)
        {
            var component = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var name = component.First();
            name = hasTriggerText ? name[TriggerText.Length..] : name;
            var aggregatedCommands = await commandStore.GetAggregatedCommands();
            if (aggregatedCommands.ContainsKey(name))
            {
                var command = aggregatedCommands[name];
                if (component.Length > 1)
                {
                    if (string.IsNullOrEmpty(command.OldArguments))
                        command.OldArguments = component.Last();
                    command.NewArguments = component.Last();
                }
                else
                {
                    command.NewArguments = null;
                }
                return command;
            }

            return null;
        }

        #region Properties

        public string TriggerText => configProvider.Configuration!.Discord!.TriggerText!;

        #endregion
    }
}
