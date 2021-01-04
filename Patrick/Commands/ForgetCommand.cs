using Patrick.Models;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ForgetCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public ForgetCommand(ICommandStore commandStore) : base("forget")
        {
            this.commandStore = commandStore;

            Description = $"Forgets a custom command.";
            Usage = $"!{Name} `<command_name>`.";
        }

        internal override async Task<CommandResponse> PerformAction(User user)
        {
            if (user.MessageArgument == null)
            {
                return new CommandResponse(Name, "Cannot process null argument");
            }

            var customCommands = await commandStore.GetCustomCommands();
            if (customCommands.TryGetValue(user.MessageArgument, out var command))
            {
                var result = await commandStore.RemoveCustomCommand(command);
                if (result)
                    return new CommandResponse(Name, $"Successfully forget {command.Name}");
                else
                    return new CommandResponse(Name, $"Error forgetting command {command.Name}");
            }

            return new CommandResponse(Name, $"Cannot find the command `{user.MessageArgument}`");
        }
    }
}
