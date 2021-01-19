using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Patrick.Services.Implementation
{
    class AudioService : IAudioService
    {
        private LavalinkNode? lavaLink;

        public void Configure(DiscordSocketClient client)
        {
            lavaLink = new LavalinkNode(new LavalinkNodeOptions
            {
                RestUri = "http://localhost:8090/",
                WebSocketUri = "ws://localhost:8090/",
                Password = "youshallnotpass",
                AllowResuming = true,
                BufferSize = 1024 * 1024, // 1 MiB
                DisconnectOnStop = false,
                ReconnectStrategy = ReconnectStrategies.DefaultStrategy,
                DebugPayloads = true
            }, new DiscordClientWrapper(client));

            client.Ready += Client_Ready;
        }

        public async Task Join(ulong guildId, ulong voiceChannelId, string youtubeLink)
        {
            if (lavaLink == null)
                throw new InvalidOperationException("Audio Service not cofigured yet");

            // get player
            var player = lavaLink.GetPlayer<LavalinkPlayer>(guildId)
                ?? await lavaLink.JoinAsync(guildId, voiceChannelId);

            // resolve a track from youtube
            var track = await lavaLink.GetTrackAsync(youtubeLink, SearchMode.YouTube);

            if (track == null)
                throw new KeyNotFoundException($"Cannot find link: {youtubeLink}");

            // play track
            await player.PlayAsync(track);
        }

        private async Task Client_Ready()
        {
            try
            {
                await lavaLink!.InitializeAsync();
                await lavaLink!.ConnectAsync();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
