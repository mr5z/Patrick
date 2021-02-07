namespace Patrick.Models.Implementation
{
    class DiscordServer : IServer
    {
        public DiscordServer(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
