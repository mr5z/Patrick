using Microsoft.Extensions.DependencyInjection;
using Patrick.Models;
using Patrick.Services;
using Patrick.Services.Implementation;
using Patrick.Services.Repositories;
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

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddLogging()
                .AddSingleton<IRepository>(_ => new MonkeyCacheRepository("Patrick"))
                .AddSingleton<IAppConfigProvider>(_ => new AppConfigProvider(appConfig!))
                .AddSingleton<IServiceCollection>(_ => serviceCollection)
                .AddSingleton<ICommandStore, CommandStore>()
                .AddSingleton<IHttpService, HttpService>()
                .AddTransient<IChatService, DiscordService>()
                .AddTransient<ICommandParser, CommandParser>()
                .AddTransient<IUserService, UserService>()
                .AddTransient<IUserFactory, DiscordUserFactory>()
                .BuildServiceProvider();

            return serviceProvider!;
        }
    }
}
