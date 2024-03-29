﻿using Microsoft.Extensions.DependencyInjection;
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
        private const string GistsCollectionName = "Gists";

        private readonly IRepository repository;
        private readonly IServiceCollection serviceCollection;
        private readonly IGistGithubService gistService;

        private readonly HashSet<BaseCommand> nativeCommands = new HashSet<BaseCommand>();
        private bool nativeCommandsPopulated;

        public CommandStore(
            IRepository repository,
            IServiceCollection serviceCollection,
            IGistGithubService gistService)
        {
            this.repository = repository;
            this.serviceCollection = serviceCollection;
            this.gistService = gistService;
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
            return repository.Add(CollectionName, command, cancellationToken);
        }

        public Task<bool> RemoveCustomCommand(CustomCommand command, CancellationToken cancellationToken)
        {
            return repository.Remove(CollectionName, command, cancellationToken);
        }

        public Task<bool> UpdateCustomCommand(CustomCommand command, CancellationToken cancellationToken)
        {
            return repository.Update(CollectionName, command.Name, command, cancellationToken);
        }

        public async Task<BaseCommand?> FindCommand(string name, CancellationToken cancellationToken)
        {
            var commandList = await GetAggregatedCommands(cancellationToken);

            if (commandList.TryGetValue(name, out var command))
                return command;

            return null;
        }

        public void ClearCommands()
        {
            repository.Clear(CollectionName);
        }

        public async Task<Dictionary<string, CustomCommand>> GetCustomCommands(CancellationToken cancellationToken)
        {
            var list = await repository.GetList<CustomCommand>(CollectionName, cancellationToken);
            return list.ToDictionary(e => e.Name, e => e);
        }

        public async Task<HashSet<BaseCommand>> GetNativeCommands(CancellationToken cancellationToken)
        {
            await Task.Run(PopulateNativeCommands, cancellationToken);
            return nativeCommands;
        }


        public async Task<string?> GetStoreId()
        {
            var gistIds = await repository.GetList<string?>(GistsCollectionName);
            return gistIds.FirstOrDefault();
        }

        public async Task<bool> SetStoreId(string id)
        {
            var result = await repository.Add(GistsCollectionName, id);
            return !string.IsNullOrEmpty(result);
        }

        public async Task<Dictionary<string, BaseCommand>> GetAggregatedCommands(CancellationToken cancellationToken)
        {
            var customCommands = await GetCustomCommands(cancellationToken);
            var nativeCommands = await GetNativeCommands(cancellationToken);
            var aggregatedCommands = new Dictionary<string, BaseCommand>(nativeCommands.ToDictionary(e => e.Name, e => e));
            foreach (var cmd in customCommands)
            {
                aggregatedCommands[cmd.Key] = cmd.Value;
            }
            return aggregatedCommands;
        }
    }
}
