using Patrick.Enums;
using System.Text;

namespace Patrick.Helpers
{
    static class RoleHelper
    {
        public static string GenerateEmojiRoles(Role role)
        {
            if (role == Role.FullAccess)
                return ":star:";

            if (role ==Role.None)
                return ":man_shrugging:";

            var builder = new StringBuilder();
            if (role.HasFlag(Role.Read))
                builder.Append(":regional_indicator_r:");
            if (role.HasFlag(Role.Write))
                builder.Append(":regional_indicator_w:");
            if (role.HasFlag(Role.Delete))
                builder.Append(":regional_indicator_d:");
            if (role.HasFlag(Role.ManageRoles))
                builder.Append("(:regional_indicator_m::regional_indicator_r:)");
            if (role.HasFlag(Role.ManageUsers))
                builder.Append("(:regional_indicator_m::regional_indicator_u:)");

            return builder.ToString();
        }
    }
}
