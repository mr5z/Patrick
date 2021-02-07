using Patrick.Enums;
using System.Collections.Generic;

namespace Patrick.Models
{
    interface IUser
    {
        ulong Id { get; }
        IChannel CurrentChannel { get; }
        IServer CurrentServer { get; }
        string? Fullname { get; set; }
        string? MessageArgument { get; set; }
        ulong SessionId { get; set; }
        bool IsAudible { get; set; }
        Role Role { get; set; }
        UserStatus Status { get; set; }
        IReadOnlyList<IUser> MentionedUsers { get; set; }
    }
}