using Patrick.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface ICommandStore
    {
        Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken = default);
        Task<HashSet<CustomCommand>> GetCommands(CancellationToken cancellationToken = default);
        void ClearCommands();
    }
}
