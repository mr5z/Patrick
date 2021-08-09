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
            var audioService = new AudioService();
            var eventPropagator = new EventPropagator();

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddLogging()
                .AddSingleton<IRepository>(_ => new MonkeyCacheRepository("Patrick"))
                .AddSingleton<IAppConfigProvider>(_ => new AppConfigProvider(appConfig!))
                .AddSingleton<IServiceCollection>(_ => serviceCollection)
                .AddSingleton<IAudioService>(_ => audioService)
                .AddSingleton<ICommandStore, CommandStore>()
                .AddSingleton<IAfkStore, AfkStore>()
                .AddSingleton<IHttpService, HttpService>()
                .AddSingleton<IEventPropagator>(_ => eventPropagator)
                .AddTransient<IChatService, DiscordService>()
                .AddTransient<ICommandParser, CommandParser>()
                .AddTransient<IUserService, UserService>()
                .AddTransient<IUserFactory, DiscordUserFactory>()
                .AddTransient<IGistGithubService, GistGithubService>()
                .AddTransient<ICredentialStore, CredentialStore>()
                .BuildServiceProvider();

            return serviceProvider!;
        }
    }
}
