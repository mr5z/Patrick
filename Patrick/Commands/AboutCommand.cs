using Patrick.Models;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class AboutCommand : BaseCommand
    {
        private readonly ICommandParser commandParser;

        public AboutCommand(ICommandParser commandParser) : base("about")
        {
            this.commandParser = commandParser;

            Description = "This is a bot.";
            Usage = $"To learn more about specific commands, type: !{Name} *[command_name]*";
            UseEmbed = true;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (user.MessageArgument == null)
                return new CommandResponse(Name, Information);

            var command = await commandParser.Parse(user.MessageArgument, false);
            if (command != null)
                return new CommandResponse(command.Name, command.Information);
            else
                return new CommandResponse(Name,
                    $"Cannot get information for argument {user.MessageArgument}");
        }
    }
}
