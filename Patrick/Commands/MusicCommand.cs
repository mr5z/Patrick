using Patrick.Models;
using Patrick.Services;
using System;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class MusicCommand : BaseCommand
    {
        private readonly IAudioService audioService;

        public MusicCommand(IAudioService audioService) : base("music")
        {
            this.audioService = audioService;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing arguments.");

            await Task.Delay(TimeSpan.FromSeconds(2));

            await audioService.Join(user.Id, user.CurrentChannel.Id, user.MessageArgument);

            return new CommandResponse(Name, "Work in Progress.");
        }
    }
}
