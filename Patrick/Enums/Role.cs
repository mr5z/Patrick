using System;

namespace Patrick.Enums
{
    [Flags]
    enum Role
    {
        Read = 1,
        Write = 2,
        Remove = 4,
        FullAccess = Read | Write | Remove
    }
}
