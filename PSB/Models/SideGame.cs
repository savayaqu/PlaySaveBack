using System.Text.Json.Serialization;
using PSB.Interfaces;

namespace PSB.Models
{
    public class SideGame : IGame
    {
        [JsonPropertyOrder(0)]
        [JsonPropertyName("type")]
        public string Type { get; set; } = "SideGame"; 
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }

    }
}
