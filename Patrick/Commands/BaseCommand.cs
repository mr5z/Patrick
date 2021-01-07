using Patrick.Enums;
using Patrick.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    abstract class BaseCommand : IEquatable<BaseCommand?>
    {
        public BaseCommand(string name)
        {
            Name = name;
        }

        internal abstract Task<CommandResponse> PerformAction(IUser user);

        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public string Information => @$"
**Description**: {Description}

**Usage**: {Usage}

**Author**: {Author ?? "Admin"}
**Required roles**: {GenerateEmojiRoles()}
";
        public bool IsNative { get; protected set; } = true;
        public string? OldArguments { get; set; }
        public string? NewArguments { get; set; }
        public string? Author { get; set; }
        public bool UseEmbed { get; set; }
        public Role RoleRequirement { get; set; } = Role.Read;
        public List<string> Aliases { get; set; } = new List<string>();

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

        private string GenerateEmojiRoles()
        {
            if (RoleRequirement == Role.FullAccess)
                return ":star:";

            var builder = new StringBuilder();
            if (RoleRequirement.HasFlag(Role.Read))
                builder.Append(":regional_indicator_r:");
            if (RoleRequirement.HasFlag(Role.Write))
                builder.Append(":regional_indicator_w:");
            if (RoleRequirement.HasFlag(Role.Delete))
                builder.Append(":regional_indicator_d:");

            return builder.ToString();
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
