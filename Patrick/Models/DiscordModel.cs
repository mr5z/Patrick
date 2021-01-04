namespace Patrick.Models
{
    class DiscordModel
    {
        public string? Token { get; set; }
        public string? TriggerText { get; set; }
        public ulong BotId { get; set; }
        public double TypingDuration { get; set; }
        public ulong[]? KnownChannels { get; set; }
    }
}
