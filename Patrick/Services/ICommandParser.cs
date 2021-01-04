using Patrick.Commands;
using System.Threading.Tasks;

namespace Patrick.Services
{
    interface ICommandParser
    {
        Task<BaseCommand?> Parse(string text, bool hasTriggerText = true);
    }
}
