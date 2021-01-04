using Patrick.Services;
using System;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class LearnCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public LearnCommand(ICommandStore commandStore) : base("learn")
        {
            this.commandStore = commandStore;
            Description = "Remembers the command you will teach it.";
        }

        internal override Task<string> PerformAction(string? argument)
        {
            Console.WriteLine("doing something...");
            return Task.FromResult(string.Empty);
        }
    }
}
