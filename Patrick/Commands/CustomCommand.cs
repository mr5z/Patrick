using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class CustomCommand : BaseCommand
    {
        public CustomCommand(string name) : base(name)
        {
            IsNative = false;
        }

        internal override Task<string> PerformAction(string? argument)
        {
            throw new NotImplementedException();
        }
    }
}
