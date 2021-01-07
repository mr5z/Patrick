using Patrick.Enums;
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
            RoleRequirement = Role.Delete;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (user.MessageArgument == null)
                return new CommandResponse(Name, "Cannot process null argument");

            var genericCommand = await commandStore.FindCommand(user.MessageArgument);
            if (genericCommand is CustomCommand command)
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
