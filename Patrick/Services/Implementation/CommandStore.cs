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

        private IServiceProvider? serviceProvider;

        private readonly HashSet<BaseCommand> nativeCommands = new HashSet<BaseCommand>();

        public CommandStore(IRepository repository)
        {
            this.repository = repository;

            PopulateNativeCommands();
        }

        private void PopulateNativeCommands()
        {
            var implementers = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(it =>
                        typeof(BaseCommand).IsAssignableFrom(it) &&
                        !typeof(CustomCommand).IsAssignableFrom(it) &&
                        !it.IsAbstract
                    )
                );

            var serviceCollection = new ServiceCollection();

            foreach (var type in implementers)
            {
                serviceCollection.AddTransient(type);
            }
            serviceProvider = serviceCollection.BuildServiceProvider();

            var assemblyName = typeof(Program).Assembly.GetName().Name;
            foreach (var type in implementers)
            {
                var command = serviceProvider.GetService(Type.GetType(type.FullName!));
                nativeCommands.Add((BaseCommand)command);
            }
        }

        private BaseCommand? CreateCommandInstance(Type type)
        {
            var parameterTypes = type
                .GetConstructors()
                .First()
                .GetParameters()
                .Select(e => e.ParameterType);

            if (parameterTypes.Any())
            {
                return (BaseCommand?)Activator.CreateInstance(type, parameterTypes.ToArray());
            }
            else
            {
                return (BaseCommand?)Activator.CreateInstance(type);
            }

        }

        public Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken)
        {
            return repository.Add(command.Name, command, cancellationToken);
        }

        public void ClearCommands()
        {
            repository.Clear(CollectionName);
        }

        public Task<HashSet<CustomCommand>> GetCommands(CancellationToken cancellationToken)
        {
            return repository.GetList<CustomCommand>(CollectionName, cancellationToken);
        }
    }
}
