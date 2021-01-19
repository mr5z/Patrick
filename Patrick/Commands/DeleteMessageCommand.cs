using Patrick.Enums;
using Patrick.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class DeleteMessageCommand : BaseCommand
    {
        public DeleteMessageCommand() : base("delete")
        {
            RoleRequirement = Role.Delete;
            Description = "Deletes messages from this channel.";
            Usage = @$"
To use, type `!{Name} [id <message ID>]|[<count>]`
By default, count is set to 10 if you didn't specify an argument.
".Trim();
        }

        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            var messageArgs = user.MessageArgument;

            if (!string.IsNullOrEmpty(messageArgs) && messageArgs.StartsWith("id"))
            {
                try
                {
                    var stringId = messageArgs.Replace("id", "").Trim();
                    bool deleteResult = false;
                    if (ulong.TryParse(stringId, out var messageId))
                        deleteResult = await user.CurrentChannel.DeleteMessage(messageId);

                    return new CommandResponse(Name,
                        deleteResult ? $"Successfully deleted the message with id {messageId}" :
                        $"There's some problem deleting the message with id {messageId}");
                }
                catch (System.Exception ex)
                {
                    return new CommandResponse(Name, ex.Message, ("cs", "```"));
                }
            }

            if (!int.TryParse(messageArgs, out var messageCount))
                messageCount = 10;

            var messages = await user.CurrentChannel.GetMessages(messageCount);
            var taskList = new List<Task<bool>>();
            foreach (var msg in messages)
            {
                var task = user.CurrentChannel.DeleteMessage(msg.Id);
                taskList.Add(task);
            }

            var result = await Task.WhenAll(taskList);

            return new CommandResponse(Name, 
                result.All(e => e) ? $"Deleted {messages.Count} messages." :
                "There's some problem deleting the messages.");
        }
    }
}