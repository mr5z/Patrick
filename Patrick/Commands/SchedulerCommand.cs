using Patrick.Models;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class SchedulerCommand : BaseCommand
    {
        public SchedulerCommand() : base("scheduler")
        {
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            return new CommandResponse(Name, "Coming soon.");
        }
    }
}
