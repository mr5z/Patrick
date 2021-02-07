using Patrick.Enums;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Patrick.Helpers.CliHelper;

namespace Patrick.Commands
{
    // TODO refactor this!
    class UpdateCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public UpdateCommand(ICommandStore commandStore) : base("update")
        {
            this.commandStore = commandStore;
            RoleRequirement = Role.Write;
            Description = "Updates existing custom command's description and usage texts.";
            Usage = @$"
!{Name} <command_name> -u 'New usage' -d 'New description'

• **-u** / **--usage** - A text providing how to use the custom command. Can be enclosed within ' or "" (single or double quotes).
• **-d** / **--description** - A text providing some info about the custom command. Can be enclosed within ' or "" (single or double quotes).
".Trim();
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing argument.");

            var helper = Helper.Parse(user.MessageArgument);
            var genericCommand = await commandStore.FindCommand(helper.CommandName);
            if (genericCommand is CustomCommand command)
            {
                command.Description = helper.Description ?? command.Description;
                command.Usage = helper.Usage ?? command.Usage;
                var result = await commandStore.UpdateCustomCommand(command);
                if (result)
                    return new CommandResponse(Name, $"Successfully updated command {command.Name}");
                else
                    return new CommandResponse(Name, $"Failed to update command {command.Name}");
            }

            return new CommandResponse(Name, $"Cannot find the command {user.MessageArgument}");
        }

        class Helper
        {
            enum Parameters { Description, Usage }
            public Helper(string name)
            {
                CommandName = name;
            }
            public string CommandName { get; }
            public string? Description { get; set; }
            public string? Usage { get; set; }

            public static Helper Parse(string text)
            {
                var components = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var commandName = components.First();
                if (components.Length <= 1)
                    return new Helper(commandName);

                var optionString = components.Last();
                var option = ParseOptions(optionString,
                    new Option<Parameters>(Parameters.Description, "-d", "--description"),
                    new Option<Parameters>(Parameters.Usage, "-u", "--usage")
                );

                return new Helper(commandName)
                {
                    Description = option.TryGetFirst(Parameters.Description, out var description) ? description : null,
                    Usage = option.TryGetFirst(Parameters.Usage, out var usage) ? usage : null
                };
            }
        }
    }
}
