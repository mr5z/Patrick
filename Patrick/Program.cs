using Microsoft.Extensions.DependencyInjection;
using Patrick.Services;
using System;
using System.Threading.Tasks;

namespace Patrick
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = await Bootstrap.Initialize(args);
            var chatService = serviceProvider.GetRequiredService<IChatService>();

            if (chatService != null)
            {
                await chatService.Start();
            }
            else
            {
                Console.WriteLine("Cannot instantiate ChatService! Quitting...");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}
