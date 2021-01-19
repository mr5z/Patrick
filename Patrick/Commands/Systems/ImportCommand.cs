using Patrick.Enums;
using Patrick.Models;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ImportCommand : BaseCommand
    {
        private const string FileName = "patrick-stack-commands.json";

        private readonly ICommandStore commandStore;
        private readonly IGistGithubService gistService;

        public ImportCommand(ICommandStore commandStore, IGistGithubService gistService) : base("import")
        {
            this.commandStore = commandStore;
            this.gistService = gistService;

            RoleRequirement = Role.FullAccess;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            return new CommandResponse(Name, "Coming soon");
            //var gistIds = await repository.GetList<string>(GistsCollectionName);
            //var id = gistIds.FirstOrDefault();
            //if (id == default)
            //    return false;

            //var gist = await gistService.Find(id);
            //if (gist == null)
            //    return false;

            //var commandList = JsonSerializer.Deserialize<Dictionary<string, BaseCommand>>(gist.Content);
            //if (commandList == null)
            //    return false;

            //ClearCommands();
            //var taskList = new List<Task<string?>>();
            //foreach (var entry in commandList)
            //{
            //    if (entry.Value is CustomCommand command)
            //    {
            //        var task = AddCustomCommand(command, default);
            //        taskList.Add(task);
            //    }
            //}

            //var result = await Task.WhenAll(taskList);

            //return !result.Any(string.IsNullOrEmpty);
        }
    }
}
