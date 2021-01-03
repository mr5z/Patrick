using Patrick.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patrick.Helpers
{
    static class CommandParser
    {
        public static BaseCommand? Parse(string text)
        {
            var component = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var name = component.First();

            return null;
        }
    }
}
