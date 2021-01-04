using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Patrick.Models
{
    class AppConfiguration
    {
        public static async Task<AppConfiguration?> LoadFrom(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<AppConfiguration>(stream);
        }
        [JsonPropertyName("Discord")]
        public DiscordModel? Discord { get; set; }
    }
}
