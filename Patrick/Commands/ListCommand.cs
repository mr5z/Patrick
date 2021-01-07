using Patrick.Models;
using Patrick.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ListCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public ListCommand(ICommandStore commandStore) : base("list")
        {
            this.commandStore = commandStore;
            Description = "List all native and custom commands";
            Usage = $"!{Name}";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            var message = await GetMessage();
            return new CommandResponse(Name, message);
        }

        private async Task<string> GetMessage()
        {
            var nativeCommands = await commandStore.GetAggregatedCommands();
            var message = $"My native commands are: `{string.Join(", ", nativeCommands.Where(e => e.Value.IsNative).Select(e => e.Key))}`\n\n";
            message += $"Current custom commands: `{string.Join(", ", nativeCommands.Where(e => !e.Value.IsNative).Select(e => e.Key))}`";
            return message;
        }
    }
}
