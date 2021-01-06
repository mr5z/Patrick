using System;

namespace Patrick.Enums
{
    [Flags]
    enum Role
    {
        Read = 1,
        Write = 2,
        Delete = 4,
        FullAccess = Read | Write | Delete
    }
}
