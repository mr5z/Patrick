using Patrick.Models;
using Patrick.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ClearUserCommand : BaseCommand
    {
        private readonly IUserService userService;

        public ClearUserCommand(IUserService userService) : base("clearuser")
        {
            this.userService = userService;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
            {
                await userService.ClearUsers();
                return new CommandResponse(Name, "All cached users cleared.");
            }

            if (!user.MentionedUsers.Any())
                return new CommandResponse(Name, "You must either mention at least one user or supply no arguments for this command to work.");

            var taskList = new List<Task<bool>>();
            foreach(var mentioned in user.MentionedUsers)
            {
                var task = userService.RemoveUser(mentioned);
                taskList.Add(task);
            }

            var result = await Task.WhenAll(taskList);

            if (result.Any(e => !e))
                return new CommandResponse(Name, "Command doesn't executed properly.");
            else
                return new CommandResponse(Name, "All mentioned users have been remove from cache.");
        }
    }
}
