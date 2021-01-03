using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Patrick.Models
{
    class AppConfiguration
    {
        public static async Task<AppConfiguration?> LoadFrom(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<AppConfiguration>(stream);
        }

        public Discord? Discord { get; set; }
    }

    public class Discord
    {
        public string? Token { get; set; }
        public string? TriggerText { get; set; }
    }
}
