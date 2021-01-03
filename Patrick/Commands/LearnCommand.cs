using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Patrick.Commands
{
    class LearnCommand : BaseCommand
    {
        public LearnCommand() : base("learn")
        {
        }

        internal override Task<bool> PerformAction()
        {
            throw new NotImplementedException();
        }
    }
}
