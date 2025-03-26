using System;
using System.Text.Json.Serialization;
using PSB.Converters;
using PSB.Models;

namespace PSB.Api.Response
{
    public class LibraryResponse
    {
        [JsonPropertyName("id")] public required int Id { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        [JsonPropertyName("last_played_at")] public DateTime? LastPlayedAt { get; set; }
        [JsonPropertyName("time_played")] public uint? TimePlayed { get; set; }
        [JsonPropertyName("is_favorite")] public Boolean IsFavorite { get; set; }
        [JsonPropertyName("game")] public Game? Game { get; set; }


    }
}
