﻿using Patrick.Enums;
using Patrick.Helpers;
using Patrick.Models;
using Patrick.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class UserCommand : BaseCommand
    {
        private readonly IUserService userService;

        public UserCommand(IUserService userService) : base("user")
        {
            this.userService = userService;

            UseEmbed = true;
            Description = "User utility.";
            Usage = @"
Options:
-f / --find     <name> Find the user with supplied argument.
-l / --list     <active/inactive/all> List the users based on the supplied argument.
".Trim();
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Argument is null");

            var helper = Helper.Parse(user.MessageArgument);

            var outputBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(helper.StatusToList))
            {
                if (string.Equals("active", helper.StatusToList, StringComparison.OrdinalIgnoreCase))
                {
                    var activeUsers = await user.CurrentChannel.GetActiveUsers();
                    var userListText = string.Join('\n',
                        activeUsers.OrderBy(e => e.Status).Select(e =>
                        $"{(e.Status == Enums.UserStatus.Online ? ":green_circle:" : ":red_circle:")} __{e.Fullname}__")
                    );
                    outputBuilder.AppendLine("===============================");
                    outputBuilder.AppendLine(userListText);
                    outputBuilder.AppendLine();
                }
            }

            if (!string.IsNullOrEmpty(helper.NameToFind))
            {
                var activeUsers = await user.CurrentChannel.GetActiveUsers();
                var cachedUsers = await userService.GetUsers();
                bool comparer(IUser user) =>
                    user.Fullname?.Contains(helper.NameToFind, StringComparison.OrdinalIgnoreCase) ?? false;
                var cachedUser = cachedUsers.FirstOrDefault(comparer);
                var snowflake = activeUsers.FirstOrDefault(comparer) ?? cachedUser;

                if (snowflake != default)
                {
                    snowflake.Role = cachedUser?.Role ?? (Role.Write | Role.Read);
                    outputBuilder.AppendLine(@$"
Closest match with the name __{helper.NameToFind}__:

**Id**: {snowflake.Id}
**Name**: {snowflake.Fullname}
**Roles**: {RoleHelper.GenerateEmojiRoles(snowflake.Role)}
");
                }
                else
                {
                    outputBuilder.Append($"No active user found that matches the name *{helper.NameToFind}*");
                }
            }

            if (outputBuilder.Length == 0)
                return new CommandResponse(Name, "Empty args.");

            return new CommandResponse(Name, outputBuilder.ToString());
        }

        class Helper
        {
            enum Parameters { Find, List }
            public static Helper Parse(string text)
            {
                var option = CliHelper.ParseOptions(text,
                    new CliHelper.Option<Parameters>(Parameters.Find, "-f", "--find"),
                    new CliHelper.Option<Parameters>(Parameters.List, "-l", "--list")
                );

                return new Helper
                {
                    NameToFind = option.TryGetFirst(Parameters.Find, out var nameToFind) ? nameToFind : null,
                    StatusToList = option.TryGetFirst(Parameters.List, out var statusToList) ? statusToList : null
                };
            }

            public string? NameToFind { get; set; }
            public string? StatusToList { get; set; }
        }
    }
}
