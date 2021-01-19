using Discord.WebSocket;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface IAudioService
    {
        void Configure(DiscordSocketClient client);
        Task Join(ulong guildId, ulong voiceChannelId, string youtubeLink);
    }
}
