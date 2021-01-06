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


        enum Option { FindIdByName, ListActiveUsers }
        class Helper
        {
            public static Helper Parse(string text)
            {
                var option = ParseOptions(text);

                return new Helper()
                {
                    NameToFind = option[Option.FindIdByName]
                };
            }

            private static Dictionary<Option, string?> ParseOptions(string text)
            {
                var dictionary = DefaultOptions();
                var combinedOptions = CliHelper.CombineOption(text.Split(' ', StringSplitOptions.RemoveEmptyEntries), ' ');
                var queue = new Queue<string?>(combinedOptions);
                while (queue.Count > 0)
                {
                    var entry = queue.Dequeue();
                    if (entry == "-f" || entry == "--findIdByName")
                        dictionary[Option.FindIdByName] = queue.Dequeue();
                    else if (entry == "-l" || entry == "--listActiveUsers")
                        dictionary[Option.ListActiveUsers] = queue.Dequeue();
                }
                return dictionary;
            }

            private static Dictionary<Option, string?> DefaultOptions()
            {
                return new Dictionary<Option, string?>
                {
                    [Option.FindIdByName] = null,
                    [Option.ListActiveUsers] = null
                };
            }

            public string? NameToFind { get; set; }
        }
    }
}
