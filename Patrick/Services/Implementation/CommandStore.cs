using Microsoft.Extensions.DependencyInjection;
using Patrick.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class CommandStore : ICommandStore
    {
        private const string CollectionName = "Commands";

        private readonly IRepository repository;
        private readonly IServiceCollection serviceCollection;

        private readonly HashSet<BaseCommand> nativeCommands = new HashSet<BaseCommand>();
        private bool nativeCommandsPopulated;

        public CommandStore(IRepository repository, IServiceCollection serviceCollection)
        {
            this.repository = repository;
            this.serviceCollection = serviceCollection;
        }

        private void PopulateNativeCommands()
        {
            if (nativeCommandsPopulated)
                return;

            nativeCommandsPopulated = true;

            var implementers = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(it =>
                        typeof(BaseCommand).IsAssignableFrom(it) &&
                        !typeof(CustomCommand).IsAssignableFrom(it) &&
                        !it.IsAbstract
                    )
                );

            foreach (var type in implementers)
            {
                serviceCollection.AddTransient(type);
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var assemblyName = typeof(Program).Assembly.GetName().Name;

            foreach (var type in implementers)
            {
                var command = (BaseCommand)serviceProvider.GetService(type);
                nativeCommands.Add(command);
            }
        }

        public Task<string?> AddCustomCommand(CustomCommand command, CancellationToken cancellationToken)
        {
            return repository.Add(command.Name, command, cancellationToken);
        }

        public void ClearCommands()
        {
            repository.Clear(CollectionName);
        }

        public async Task<Dictionary<string, BaseCommand>> GetCustomCommands(CancellationToken cancellationToken)
        {
            var list = await repository.GetList<BaseCommand>(CollectionName, cancellationToken);
            return list.ToDictionary(e => e.Name, e => e);
        }

        public async Task<HashSet<BaseCommand>> GetNativeCommands(CancellationToken cancellationToken)
        {
            await Task.Run(PopulateNativeCommands, cancellationToken);
            return nativeCommands;
        }

        public async Task<Dictionary<string, BaseCommand>> GetAggregatedCommands(CancellationToken cancellationToken)
        {
            var customCommands = await GetCustomCommands(cancellationToken);
            var nativeCommands = await GetNativeCommands(cancellationToken);
            foreach (var cmd in nativeCommands)
            {
                customCommands[cmd.Name] = cmd;
            }
            return customCommands;
        }
    }
}
