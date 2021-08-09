using Patrick.Helpers;
using Patrick.Models;
using Patrick.Models.Events;
using Patrick.Services;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class AfkCommand : BaseCommand
    {
        private readonly IAfkStore afkStore;
        private readonly IUserService userService;

        public AfkCommand(
            IEventPropagator eventPropagator,
            IAfkStore afkStore,
            IUserService userService) : base("afk")
        {
            this.afkStore = afkStore;
            this.userService = userService;

            Description = "Remind someone that you're AFK when they mentioned you.";
            Usage = $@"
!{Name} --set <Message for user going to mention you>
!{Name} --clear - Will clear the note message
".Trim();

            eventPropagator.UserMentioned -= EventPropagator_UserMentioned;
            eventPropagator.UserMentioned += EventPropagator_UserMentioned;
        }

        private async void EventPropagator_UserMentioned(object? sender, UserMentionEventArgs e)
        {
            foreach(var userId in e.UserIds)
            {
                var result = await afkStore.FindStoreMessageFor(userId);
                if (result == null)
                    continue;

                var isSuccess = await e.Channel.SendMessage(
                    new CommandResponse(Name, $@"
AFK! Note from **{result.AuthorName}**:
```
{result.Message}
```
".Trim())
                );
            }
        }

        enum Mode { Set, Clear }
        internal override async Task<CommandResponse> PerformAction(IUser user)
        {
            await afkStore.ClearAllMessages();
            if (string.IsNullOrEmpty(user.MessageArgument))
                return new CommandResponse(Name, "Missing arguments");

            var options = CliHelper.ParseOptions(user.MessageArgument,
                new CliHelper.Option<Mode>(Mode.Set, "-s", "--set"),
                new CliHelper.Option<Mode>(Mode.Clear, "-c", "--clear")
            );

            if (options.TryGetFirst(Mode.Set, out var setValue))
            {
                if (string.IsNullOrEmpty(setValue))
                    return new CommandResponse(Name, "Missing note for other users");

                await afkStore.StoreMessageFor(user.Id, user.Fullname, setValue);
            }

            if (options.TryGetFirst(Mode.Clear, out var _))
                await afkStore.ClearMessageFor(user.Id);

            return new CommandResponse(Name, "...");
        }
    }
}
