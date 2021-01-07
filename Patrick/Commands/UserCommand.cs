using Patrick.Helpers;
using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class UserCommand : BaseCommand
    {
        public UserCommand() : base("user")
        {
            Description = "User utility.";
            Usage = @"
findIdByName 'Name Here'
";
            // listActiveUsers
        }

        internal override async Task<CommandResponse> PerformAction(User user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Argument is null");

            var helper = Helper.Parse(user.MessageArgument);

            return new CommandResponse(Name, "Yay!");
        }

        class Helper
        {
            enum Parameters { FindId, List }
            public static Helper Parse(string text)
            {
                var option = CliHelper.ParseOptions(text,
                    new CliHelper.Option<Parameters>(Parameters.FindId, "-f", "--find_id"),
                    new CliHelper.Option<Parameters>(Parameters.List, "-l", "--list")
                );

                return new Helper()
                {
                    NameToFind = option[Parameters.FindId],
                    StatusToList = option[Parameters.List]
                };
            }

            public string? NameToFind { get; set; }
            public string? StatusToList { get; set; }
        }
    }
}
