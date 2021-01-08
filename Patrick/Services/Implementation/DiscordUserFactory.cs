using Patrick.Models;
using Patrick.Models.Implementation;

namespace Patrick.Services.Implementation
{
    class DiscordUserFactory : IUserFactory
    {
        public IUser Create(ulong userId)
        {
            return new DiscordUser(userId, new MockChannel());
        }
    }
}
