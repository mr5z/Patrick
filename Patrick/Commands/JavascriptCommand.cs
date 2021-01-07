using Jint;
using Patrick.Models;
using System;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class JavascriptCommand : BaseCommand
    {
        public JavascriptCommand() : base("js")
        {
            Description = "Executes [JavaScipt](https://en.wikipedia.org/wiki/JavaScript) code";
            Usage = $"!{Name} <javascript code>";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "No arguments found");

            var tcs = new TaskCompletionSource<CommandResponse>();
            Action<object> callback = (output) =>
            {
                var result = new CommandResponse(Name, $"```js\n{output}\n```");
                tcs.TrySetResult(result);
            };

            const string callbackName = "log";
            var engine = new Engine()
                .SetValue(callbackName, callback);

            engine.Execute($"{callbackName}({user.MessageArgument})");

            return await tcs.Task;
        }
    }
}
