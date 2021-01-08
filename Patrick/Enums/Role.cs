using System;

namespace Patrick.Enums
{
    [Flags]
    enum Role
    {
        None,
        Read = 1,
        Write = 2,
        Delete = 4,
        ManageRoles = 8,
        ManageUsers = 16,
        FullAccess = Read | Write | Delete | ManageRoles | ManageUsers
    }
}
