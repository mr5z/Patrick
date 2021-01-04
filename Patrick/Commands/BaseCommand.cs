using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    abstract class BaseCommand : IEquatable<BaseCommand?>
    {
        public BaseCommand(string name)
        {
            Name = name;
        }

        internal abstract Task<CommandResponse> PerformAction(User user);

        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public string Information => $"**Description**: {Description}\n\n**Usage**: {Usage}\n\n**Author**: {Author ?? "Admin"}";
        public bool IsNative { get; protected set; } = true;
        public string? OldArguments { get; set; }
        public string? NewArguments { get; set; }
        public string? Author { get; set; }
        public bool UseEmbed { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BaseCommand);
        }

        public bool Equals(BaseCommand? other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(BaseCommand? left, BaseCommand? right)
        {
            return EqualityComparer<BaseCommand>.Default.Equals(left, right);
        }

        public static bool operator !=(BaseCommand? left, BaseCommand? right)
        {
            return !(left == right);
        }
    }
}
