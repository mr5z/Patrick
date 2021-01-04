using Patrick.Commands;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface ICommandStore
    {
        Task<string?> AddCustomCommand(CustomCommand command, CancellationToken cancellationToken = default);
        Task<Dictionary<string, BaseCommand>> GetCustomCommands(CancellationToken cancellationToken = default);
        Task<Dictionary<string, BaseCommand>> GetAggregatedCommands(CancellationToken cancellationToken = default);
        Task<HashSet<BaseCommand>> GetNativeCommands(CancellationToken cancellationToken = default);
        void ClearCommands();
    }
}
