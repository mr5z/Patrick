using Microsoft.Extensions.DependencyInjection;
using Patrick.Services;
using System.Threading.Tasks;
using System;

namespace Patrick
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = await Bootstrap.Initialize(args);
            var discordService = serviceProvider.GetService<IDiscordService>()!;
            await discordService.Start();

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
