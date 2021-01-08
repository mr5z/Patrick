namespace Patrick.Models
{
    class CommandResponse
    {
        public CommandResponse(string commandName, string? message, (string tag, string token)? messageEnclosure = null)
        {
            CommandName = commandName;
            Message = message;
            MessageEnclosure = messageEnclosure;
        }
        public string? Message { get; }
        public string CommandName { get; }
        public (string tag, string token)? MessageEnclosure { get; set; }
    }
}
