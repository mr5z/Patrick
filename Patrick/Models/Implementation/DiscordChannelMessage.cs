namespace Patrick.Models.Implementation
{
    class DiscordChannelMessage : IChannelMessage
    {
        public DiscordChannelMessage(ulong id)
        {
            Id = id;
        }
        public ulong Id { get; }
    }
}
