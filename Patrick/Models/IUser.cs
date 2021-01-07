using Patrick.Enums;

namespace Patrick.Models
{
    interface IUser
    {
        ulong Id { get; }
        IChannel CurrentChannel { get; }
        string? Fullname { get; set; }
        string? MessageArgument { get; set; }
        ulong SessionId { get; set; }
        Role Role { get; set; }
        UserStatus Status { get; set; }
    }
}
