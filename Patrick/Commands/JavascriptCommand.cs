using Jint;
using Patrick.Models;
using System;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class JavascriptCommand : BaseCommand
    {
        private const string CallbackName = "___log___";

        private readonly Engine javascriptEngine = new Engine();

        public JavascriptCommand() : base("js")
        {
            Description = "Executes [JavaScipt](https://en.wikipedia.org/wiki/JavaScript) code";
            Usage = $"!{Name} <JavaScript code>";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "No arguments found");

            var tcs = new TaskCompletionSource<CommandResponse>();
            Action<object> callback = (output) =>
            {
                var result = new CommandResponse(Name, output.ToString(), ("js", "```"));
                tcs.TrySetResult(result);
            };

            javascriptEngine.SetValue(CallbackName, callback);

            try
            {
                javascriptEngine.Execute($"{CallbackName}({user.MessageArgument})", new Jint.Parser.ParserOptions
                {
                    Comment = true,
                    Tokens = true,
                    Tolerant = true
                });
            }
            catch (Exception ex)
            {
                return new CommandResponse(Name, ex.Message, ("js", "```"));
            }

            return await tcs.Task;
        }
    }
}
