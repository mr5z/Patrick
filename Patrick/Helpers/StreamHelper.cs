using System.IO;
using System.Threading.Tasks;

namespace Patrick.Helpers
{
    static class StreamHelper
    {
        public static async Task<string> ToString(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
