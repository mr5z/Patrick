using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Patrick.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class CsharpCommand : BaseCommand
    {
        public CsharpCommand() : base("cs")
        {
            Description = "Executes [C#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)) code.";
            Usage = $"!{Name} <insert C# code here>";
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Null argument.");

            //var proc = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "csc.exe",
            //        UseShellExecute = false,
            //        Arguments = "/r:System.dll /out:sample.exe stdstr.cs",
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    }
            //};
            //_ = await Task.Run(() => proc.Start());

            var engine = new CSharpScriptEngine();

            string.Join(", ", System.Linq.Enumerable.Range(0, 10).Select(e => $"{10 - e}"));

            object? result = null;
            try
            {
                var c = await engine.Prepare();
                if (c != null)
                    result = await c.Execute(user.MessageArgument);
            }
            catch (System.Exception ex)
            {
                var msg = ex.Message;
                return new CommandResponse(Name, $"```csharp\n{msg}\n```");
            }
            var stringBuilder = new StringBuilder();
            //string? line = null;
            //while (!string.IsNullOrEmpty(line = proc.StandardOutput.ReadLine()))
            //{
            //    stringBuilder.Append(line);
            //    // do something with line
            //}


            return new CommandResponse(Name, @$"
```csharp
{result?.ToString() ?? stringBuilder.ToString()}
```");
        }

        class CSharpScriptEngine
        {
            private ScriptState<object?>? scriptState;
            public async Task<CSharpScriptEngine> Prepare()
            {
                scriptState = await CSharpScript.RunAsync(@"
using System;
using System.Text;
using System.Linq;
",
                    ScriptOptions.Default.WithReferences(
                        typeof(System.Math).Assembly,
                        typeof(System.Text.StringBuilder).Assembly,
                        typeof(System.Linq.Enumerable).Assembly)
                    );
                return this;
            }

            public async Task<string?> Execute(string code)
            {
                if (scriptState != null)
                {
                    var result = await scriptState.ContinueWithAsync(code);
                    return result.ReturnValue?.ToString();
                }

                return null;
            }
        }
    }
}
