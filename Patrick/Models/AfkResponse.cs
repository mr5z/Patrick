using System;
using System.Collections.Generic;

namespace Patrick.Models
{
    public class AfkResponse : IEquatable<AfkResponse?>
    {
        public ulong AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public string? Message { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as AfkResponse);
        }

        public bool Equals(AfkResponse? other)
        {
            return other != null &&
                   AuthorId == other.AuthorId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AuthorId);
        }

        public static bool operator ==(AfkResponse? left, AfkResponse? right)
        {
            return EqualityComparer<AfkResponse>.Default.Equals(left, right);
        }

        public static bool operator !=(AfkResponse? left, AfkResponse? right)
        {
            return !(left == right);
        }
    }
}
