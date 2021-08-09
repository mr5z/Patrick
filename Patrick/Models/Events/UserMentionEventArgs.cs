using System;
using System.Collections.Generic;

namespace Patrick.Models.Events
{
    class UserMentionEventArgs : EventArgs
    {
        public UserMentionEventArgs(ulong authorId, IEnumerable<ulong> userIds, IChannel channel)
        {
            AuthorId = authorId;
            UserIds = userIds;
            Channel = channel;
        }

        public ulong AuthorId { get; }
        public IEnumerable<ulong> UserIds { get; }
        public IChannel Channel { get; }
    }
}
