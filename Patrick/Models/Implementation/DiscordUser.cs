using Patrick.Enums;
using System;
using System.Collections.Generic;

namespace Patrick.Models.Implementation
{
    class DiscordUser : IUser, IEquatable<DiscordUser?>
    {
        public DiscordUser(ulong id, IChannel currentChannel)
        {
            Id = id;
            CurrentChannel = currentChannel;
        }

        public ulong Id { get; }
        public string? Fullname { get; set; }
        public string? MessageArgument { get; set; }
        public ulong SessionId { get; set; }
        public Role Role { get; set; }
        public IChannel CurrentChannel { get; set; }
        public UserStatus Status { get; set; }
        public IReadOnlyList<IUser> MentionedUsers { get; set; } = new List<IUser>();

        public override bool Equals(object? obj)
        {
            return Equals(obj as DiscordUser);
        }

        public bool Equals(DiscordUser? other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(DiscordUser? left, DiscordUser? right)
        {
            return EqualityComparer<DiscordUser>.Default.Equals(left, right);
        }

        public static bool operator !=(DiscordUser? left, DiscordUser? right)
        {
            return !(left == right);
        }
    }
}
