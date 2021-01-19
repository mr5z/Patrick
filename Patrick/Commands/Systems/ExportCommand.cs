using Patrick.Enums;
using Patrick.Models;
using Patrick.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class ExportCommand : BaseCommand
    {
        private const string FileName = "patrick-stack-commands.json";

        private readonly ICommandStore commandStore;
        private readonly IGistGithubService gistService;

        public ExportCommand(ICommandStore commandStore, IGistGithubService gistService) : base("export")
        {
            this.commandStore = commandStore;
            this.gistService = gistService;

            RoleRequirement = Role.FullAccess;
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            if (!gistService.IsAuthenticated)
            {
                var isSuccess = await gistService.Authenticate();
                if (!isSuccess)
                    return new CommandResponse(Name, "Something went wrong.");
            }

            var commandList = await commandStore.GetCustomCommands();
            var content = JsonSerializer.Serialize(commandList);
            var storeId = await gistService.Create(new GistModel(FileName, content)
            {
                Description = "Patrick Star's command list"
            });

            if (!string.IsNullOrEmpty(storeId))
            {
                await commandStore.SetStoreId(storeId);
                return new CommandResponse(Name, $"Successfully exported! Returned id: {storeId}");
            }

            return new CommandResponse(Name, "Baah");
        }
    }
}
