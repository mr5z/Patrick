using Patrick.Enums;
using System;
using System.Collections.Generic;

namespace Patrick.Models
{
    class User : IEquatable<User?>
    {
        public User(ulong id, IChannel currentChannel)
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as User);
        }

        public bool Equals(User? other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(User? left, User? right)
        {
            return EqualityComparer<User>.Default.Equals(left, right);
        }

        public static bool operator !=(User? left, User? right)
        {
            return !(left == right);
        }
    }
}
