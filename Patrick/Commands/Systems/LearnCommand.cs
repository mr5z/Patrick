using Patrick.Enums;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class LearnCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;
        private readonly ICommandParser commandParser;

        public LearnCommand(ICommandStore commandStore, ICommandParser commandParser) : base("learn")
        {
            this.commandStore = commandStore;
            this.commandParser = commandParser;
            RoleRequirement = Role.Write;
            Description = "Remembers the command you taught. It can also build a \"dynamic\" command.";
            Usage = @"
- Pattern:
<URL/API address>|<plain text> <arguments, [...]>
- Example:
1. `!learn hello https://cool-api.com?query_string -t ignore -m post -c form -p data.url`
2. `!learn hello world`

For plain text format:
- The next argument (separated by space) will just repeat by the bot.
- Example:
> :face_with_raised_eyebrow: *`!learn hello world! 123`*
> :robot: *Learned the command `hello`*
> :face_with_raised_eyebrow: *!hello*
> :robot: *world! 123*

For URL format:
- The succeeding argument is assummed to be a supplement to build the API.
- The following can be use to construct the parameters:
  • **-t** / **--type** - Response type. Values can be either `ignore` or `consume`
  • **-m** / **--method** - Part of HTTP. Values can be either `get` or `post` atm
  • **-c** / **--content** - Is an HTTP header Content-Type. The values can be either of these three: `json`, `form`, `multi`
  • **-p** / **--path** - Is a JSON Path
".Trim();
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            var argument = user.MessageArgument;
            if (argument == null)
                return new CommandResponse(Name, $"Argument is null");

            var command = await commandParser.Parse(argument, false);

            if (command != null)
                return new CommandResponse(Name, "Command already exist");

            var component = argument.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var name = component.First();
            var args = component.Length > 1 ? component.Last() : string.Empty;
            var customCommand = new CustomCommand(name)
            {
                OldArguments = args,
                Author = user.Fullname
            };
            await commandStore.AddCustomCommand(customCommand);

            return new CommandResponse(Name, $"Command `{name}` learned.");
        }
    }
}
