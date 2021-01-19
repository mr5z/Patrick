using Patrick.Commands;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface ICommandStore
    {
        Task<string?> GetStoreId();
        Task<bool> SetStoreId(string id);
        Task<string?> AddCustomCommand(CustomCommand command, CancellationToken cancellationToken = default);
        Task<bool> RemoveCustomCommand(CustomCommand command, CancellationToken cancellationToken = default);
        Task<bool> UpdateCustomCommand(CustomCommand command, CancellationToken cancellationToken = default);
        Task<BaseCommand?> FindCommand(string name, CancellationToken cancellationToken = default);
        Task<Dictionary<string, CustomCommand>> GetCustomCommands(CancellationToken cancellationToken = default);
        Task<Dictionary<string, BaseCommand>> GetAggregatedCommands(CancellationToken cancellationToken = default);
        Task<HashSet<BaseCommand>> GetNativeCommands(CancellationToken cancellationToken = default);
        void ClearCommands();
    }
}
