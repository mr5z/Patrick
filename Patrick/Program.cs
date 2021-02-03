using Microsoft.Extensions.DependencyInjection;
using Patrick.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Patrick
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = await Bootstrap.Initialize(args);
            var chatService = serviceProvider.GetRequiredService<IChatService>();
            var cts = new CancellationTokenSource();
            if (chatService != null)
            {
                try
                {
                    await Task.Run(chatService.Start, cts.Token);
                }
                catch (OperationCanceledException) { }
            }
            else
            {
                Console.WriteLine("Cannot instantiate ChatService! Quitting...");
            }

            Console.ReadKey();
            cts.Cancel();
        }
    }
}
