using Patrick.Models;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class AliasCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;
        private readonly ICommandParser commandParser;

        public AliasCommand(ICommandStore commandStore, ICommandParser commandParser) : base("alias")
        {
            this.commandStore = commandStore;
            this.commandParser = commandParser;
        }

        internal override async Task<CommandResponse> PerformAction(User user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Argument is null.");

            var command = await commandParser.Parse(user.MessageArgument, false);

            return new CommandResponse(Name, "");
        }
    }
}
