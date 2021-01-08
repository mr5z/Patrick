using Patrick.Enums;
using Patrick.Helpers;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ManageRoleCommand : BaseCommand
    {
        private readonly IUserService userService;

        public ManageRoleCommand(IUserService userService) : base("managerole")
        {
            this.userService = userService;

            RoleRequirement = Role.ManageRoles;
            Description = "Role management command";
            Usage = @$"
!{Name} -a / --add | -r / --remove R|W|D|MR|MU @MentionSomeone

__R__ - Read
__W__ - Write
__D__ - Delete
__MR__ - Manage Roles
__MU__ - Manage Users (wip)";
        }

        enum Operation { Add, Remove }
        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing arguments");

            if (user.MentionedUsers.Count != 1)
                return new CommandResponse(Name, "Mentioned user should exactly be one only.");

            var regex = new Regex("<@![0-9]*>");
            var argument = regex.Replace(user.MessageArgument, "").Trim();

            var options = CliHelper.ParseOptions(argument,
                new CliHelper.Option<Operation>(Operation.Add, "-a", "--add"),
                new CliHelper.Option<Operation>(Operation.Remove, "-r", "--remove")
            );

            var userToManage = user.MentionedUsers.Single();

            if (!string.IsNullOrEmpty(options[Operation.Add]))
            {
                var newRole = AssignNewRole(options[Operation.Add]!);
                userToManage.Role = newRole;
            }

            if (!string.IsNullOrEmpty(options[Operation.Remove]))
            {
                var newRole = RemoveNewRole(options[Operation.Remove]!);
                userToManage.Role = newRole;
            }

            var roleAssignmentSuccess = await userService.AddUser(userToManage);

            var roleString = RoleHelper.GenerateEmojiRoles(userToManage.Role);

            return new CommandResponse(Name, @$"

User: __{userToManage.Fullname}__
New Role: {roleString}
Status: {(roleAssignmentSuccess ? "Success" : "Failed")}
");
        }

        private static Role AssignNewRole(string roleString)
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

        private static Role RemoveNewRole(string roleString)
        {
            var roles = roleString.Split('|');
            var newRole = Role.None;
            foreach (var r in roles)
            {
                switch (r)
                {
                    case "R":
                        newRole &= ~Role.Read;
                        break;
                    case "W":
                        newRole &= ~Role.Write;
                        break;
                    case "D":
                        newRole &= ~Role.Delete;
                        break;
                    case "MR":
                        newRole &= ~Role.ManageRoles;
                        break;
                    case "MU":
                        newRole &= ~Role.ManageUsers;
                        break;
                }
            }
            return newRole;
        }


    }
}
