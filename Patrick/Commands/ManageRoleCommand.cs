using Patrick.Enums;
using Patrick.Helpers;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ManageRoleCommand : BaseCommand
    {
        private readonly IUserService userService;
        private readonly IUserFactory userFactory;

        public ManageRoleCommand(IUserService userService, IUserFactory userFactory) : base("managerole")
        {
            this.userService = userService;
            this.userFactory = userFactory;

            RoleRequirement = Role.ManageRoles;
            Description = "Role management command";
            Usage = @$"
!{Name} <user_id> -a / --add | -r / --remove R|W|D|MR|MU

__R__ - Read
__W__ - Write
__D__ - Delete
__MR__ - Manage Roles
__MU__ - Manage Users (wip)";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing arguments");

            var userManaging = await userService.Find(user.Id);

            if (userManaging == null)
            {
                var result = await userService.AddUser(user);
                if (!result)
                    return new CommandResponse(Name, "Something went wrong. Please contact the admin.");
                userManaging = user;
            }

            var components = user.MessageArgument.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var userIdString = components.First();
            var optionString = components.Last();

            if (userIdString == optionString)
                return new CommandResponse(Name, "Invalid arguments.");

            if (!ulong.TryParse(userIdString, out var userIdToManage))
                return new CommandResponse(Name, "Invalid arguments.");

            var options = CliHelper.ParseOptions(optionString,
                new CliHelper.Option<Operation>(Operation.Add, "-a", "--add"),
                new CliHelper.Option<Operation>(Operation.Remove, "-r", "--remove")
            );

            var activeUser = await user.CurrentChannel.FindUser(userIdToManage);
            var cachedUser = await userService.Find(userIdToManage);
            activeUser ??= cachedUser ?? userFactory.Create(userIdToManage);

            if (!string.IsNullOrEmpty(options[Operation.Add]))
            {
                var newRole = GenerateNewRole(options[Operation.Add]!);
                activeUser.Role = newRole;
            }

            if (!string.IsNullOrEmpty(options[Operation.Remove]))
            {
                var newRole = GenerateNewRole(options[Operation.Remove]!);
                activeUser.Role = newRole;
            }

            var roleAssignmentSuccess = await userService.AddUser(activeUser);

            var roleString = RoleHelper.GenerateEmojiRoles(activeUser.Role);

            return new CommandResponse(Name, @$"

User: __{activeUser.Fullname}__
New Role: {roleString}
Status: {(roleAssignmentSuccess ? "Success" : "Failed")}
");
        }

        private static Role GenerateNewRole(string roleString)
        {
            var roles = roleString.Split('|');
            var newRole = Role.None;
            foreach (var r in roles)
            {
                switch (r)
                {
                    case "R":
                        newRole |= Role.Read;
                        break;
                    case "W":
                        newRole |= Role.Write;
                        break;
                    case "D":
                        newRole |= Role.Delete;
                        break;
                    case "MR":
                        newRole |= Role.ManageRoles;
                        break;
                    case "MU":
                        newRole |= Role.ManageUsers;
                        break;
                }
            }
            return newRole;
        }

        enum Operation { Add, Remove }

    }
}
