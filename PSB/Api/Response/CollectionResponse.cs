using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Models;

namespace PSB.Api.Response
{
    public class CollectionResponse
    {
        [JsonPropertyName("id")] public required int Id { get; set; }
        [JsonPropertyName("custom_game_id")] public ulong? CustomGameId { get; set; }
        [JsonPropertyName("game_id")] public ulong? GameId { get; set; }
        [JsonPropertyName("game")] public Game? Game { get; set; }

    }
}
