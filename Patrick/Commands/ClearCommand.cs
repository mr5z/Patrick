﻿using Patrick.Enums;
using Patrick.Models;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ClearCommand : BaseCommand
    {
        private readonly ICommandStore commandStore;

        public ClearCommand(ICommandStore commandStore) : base("clearcc")
        {
            this.commandStore = commandStore;
            Description = "Clears all custom commands";
            Usage = $"!{Name}";
            RoleRequirement = Role.Delete;
        }

        internal override Task<CommandResponse> PerformAction(IUser user)
        {
            commandStore.ClearCommands();
            return Task.FromResult(new CommandResponse(Name, "All commands cleared!"));
        }
    }
}
