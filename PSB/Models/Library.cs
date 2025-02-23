using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Converters;

namespace PSB.Models
{
    public class Library
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("game_id")] public required ulong GameId { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        [JsonPropertyName("last_played_at")] public DateTime? LastPlayedAt { get; set; }
        [JsonPropertyName("time_played")] public uint? TimePlayed { get; set; }
        [JsonPropertyName("is_favorite")] public required Boolean IsFavorite { get; set; }
    }
}
