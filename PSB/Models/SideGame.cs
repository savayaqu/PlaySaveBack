using System.Text.Json.Serialization;
using PSB.Interfaces;

namespace PSB.Models
{
    public class SideGame : IGame
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }

        public string Type => "SideGame";
    }
}
