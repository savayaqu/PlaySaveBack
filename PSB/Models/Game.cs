using System.Text.Json.Serialization;
using PSB.Interfaces;

namespace PSB.Models
{
    public class Game : IGame
    {
        [JsonPropertyOrder(0)]
        [JsonPropertyName("type")]
        public string Type { get; set; } = "game"; 

        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("platform")] public required int Platform { get; set; }
        [JsonPropertyName("game_code")] public required string GameCode { get; set; }
        [JsonPropertyName("header")] public string? Header { get; set; }        
        [JsonPropertyName("library_img")] public string? LibraryImg{ get; set; }
        [JsonPropertyName("catalog_img")] public string? CatalogImg{ get; set; }

    }
}
