using System.Collections.Generic;
using System.Text.Json.Serialization;
using PSB.Models;

namespace PSB.Api.Response
{
    public class GameResponse
    {
        [JsonPropertyName("game")] public Game? Game { get; set; }
        [JsonPropertyName("side_game")] public SideGame? SideGame { get; set; }
        [JsonPropertyName("library")] public Library? Library { get; set; }
        [JsonPropertyName("saves")] public List<Save>? Saves { get; set; }
    }
}
