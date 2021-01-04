using Patrick.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class AboutCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;
        private readonly ICommandParser commandParser;

        public AboutCommand(ICommandStore commandStore, ICommandParser commandParser) : base("about")
        {
            this.commandStore = commandStore;
            this.commandParser = commandParser;

            Description = "Lists all the native commands";
        }

        internal override async Task<string> PerformAction(string? argument)
        {
            var nativeCommands = await commandStore.GetNativeCommands();
            var messageResponse = string.Empty;

            if (argument == null)
            {
                messageResponse = $@"
My native commands are: `{string.Join(", ", nativeCommands.Select(e => e.Name))}`
To learn about specific command, type `!about <command_name>`
";
            }
            else
            {
                var command = await commandParser.Parse(argument!, false);
                if (command != null)
                {
                    messageResponse = $"{command.Description}";
                }
                else
                {
                    messageResponse = $"Cannot get description for argument {argument}";
                }
            }
            return messageResponse;
        }
    }
}
