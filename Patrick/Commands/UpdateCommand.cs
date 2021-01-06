using Patrick.Helpers;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class UpdateCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public UpdateCommand(ICommandStore commandStore) : base("update")
        {
            this.commandStore = commandStore;
            Description = "Updates existing custom command's description and usage texts.";
            Usage = @$"
!{Name} <command_name> -u 'New usage' -d 'New description'

• **-u** / **--usage** - A text providing how to use the custom command. Can be enclosed within ' or "" (single or double quotes).
• **-d** / **--description** - A text providing some info about the custom command. Can be enclosed within ' or "" (single or double quotes).
";
        }

        internal override async Task<CommandResponse> PerformAction(User user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing argument.");

            var updater = Updater.Parse(user.MessageArgument);
            var commandList = await commandStore.GetCustomCommands();
            if (commandList.TryGetValue(updater.Name, out var command))
            {
                command.Description ??= updater.Description;
                command.Usage ??= updater.Usage;
                var result = await commandStore.UpdateCustomCommand(command);
                if (result)
                    return new CommandResponse(Name, $"Successfully updated command {command.Name}");
                else
                    return new CommandResponse(Name, $"Failed to update command {command.Name}");
            }

            return new CommandResponse(Name, $"Cannot find the command {user.MessageArgument}");
        }

        class Updater
        {
            enum Option
            {
                Description,
                Usage
            }
            public Updater(string name)
            {
                Name = name;
            }
            public string Name { get; }
            public string? Description { get; set; }
            public string? Usage { get; set; }

            public static Updater Parse(string text)
            {
                var components = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var commandName = components.First();
                if (components.Length <= 1)
                    return new Updater(commandName);

                var optionString = components.Last();
                var option = ParseOptions(optionString);

                return new Updater(commandName)
                {
                    Description = option[Option.Description],
                    Usage = option[Option.Usage]
                };
            }

            private static Dictionary<Option, string?> ParseOptions(string text)
            {
                var dictionary = DefaultOptions();
                var combinedOptions = CliHelper.CombineOption(text.Split(' ', StringSplitOptions.RemoveEmptyEntries), ' ');
                var queue = new Queue<string?>(combinedOptions);
                while (queue.Count > 0)
                {
                    var entry = queue.Dequeue();
                    if (entry == "-d" || entry == "--description")
                        dictionary[Option.Description] = queue.Dequeue();
                    else if (entry == "-u" || entry == "--usage")
                        dictionary[Option.Usage] = queue.Dequeue();
                }
                return dictionary;
            }

            private static Dictionary<Option, string?> DefaultOptions()
            {
                return new Dictionary<Option, string?>
                {
                    [Option.Usage] = null,
                    [Option.Description] = null
                };
            }
        }
    }
}
