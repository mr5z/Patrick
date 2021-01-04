namespace Patrick.Models
{
    class CommandResponse
    {
        public CommandResponse(string commandName, string? message)
        {
            CommandName = commandName;
            Message = message;
        }
        public string? Message { get; }
        public string CommandName { get; }
    }
}
