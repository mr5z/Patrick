using Microsoft.Extensions.DependencyInjection;
using Patrick.Models;
using Patrick.Services;
using Patrick.Services.Implementation;
using Patrick.Services.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Patrick
{
    static class Bootstrap
    {
        public static async Task<ServiceProvider> Initialize(string[] args)
        {
            var stream = File.OpenRead("config.json");
            var appConfig = await AppConfiguration.LoadFrom(stream);
            var configProvider = new AppConfigProvider(appConfig!);

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IRepository>(_ => new MonkeyCacheRepository("Patrick"))
                .AddTransient<IAppConfigProvider>(_ => configProvider)
                .AddTransient<IDiscordService, DiscordService>()
                .AddTransient<ICommandStore, CommandStore>()
                .BuildServiceProvider();

            return serviceProvider!;
        }
    }
}
