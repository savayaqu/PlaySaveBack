using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PSB.Converters;

namespace PSB.Models
{
    public class Library
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("game_id")] public ulong? GameId { get; set; }
        [JsonPropertyName("side_game_id")] public ulong? SideGameId { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        [JsonPropertyName("last_played_at")] public DateTime? LastPlayedAt { get; set; }
        [JsonPropertyName("time_played")] public uint? TimePlayed { get; set; }
        [JsonPropertyName("is_favorite")] public required Boolean IsFavorite { get; set; }
        [JsonPropertyName("game")] public Game? Game { get; set; }
        [JsonPropertyName("sideGame")] public SideGame? SideGame { get; set; }

    }
}
