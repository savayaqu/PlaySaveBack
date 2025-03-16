using System.Text.Json.Serialization;
using PSB.Interfaces;

namespace PSB.Models
{
    public class Game : IGame
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("platform")] public required int Platform { get; set; }
        [JsonPropertyName("game_code")] public required string GameCode { get; set; }
        [JsonPropertyName("header")] public string? Header { get; set; }        
        [JsonPropertyName("library_img")] public string? LibraryImg{ get; set; }

        public string Type => "Game";
    }
}
