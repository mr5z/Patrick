using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    abstract class BaseCommand
    {
        public BaseCommand(string name)
        {
            Name = name;
        }
        internal abstract Task<string> PerformAction(string? argument);
        public string Name { get; }
        public string? Description { get; set; }
        public bool IsNative { get; protected set; } = true;
        public string? Arguments { get; set; }
    }
}
