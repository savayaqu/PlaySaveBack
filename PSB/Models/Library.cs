using System;
using System.Text.Json.Serialization;
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
        [JsonIgnore]
        public string LastPlayedText
        {
            get
            {
                if (!LastPlayedAt.HasValue)
                    return "Никогда";

                var lastPlayed = LastPlayedAt.Value;
                var difference = DateTime.Now - lastPlayed;
                var days = difference.TotalDays;

                return days switch
                {
                    < 1 => "Сегодня",
                    < 2 => "Вчера",
                    <= 6 => $"{Math.Floor(days)} дней назад",
                    <= 7 => "Неделю назад",
                    <= 14 => "2 недели назад",
                    <= 21 => "3 недели назад",
                    <= 28 => "4 недели назад",
                    <= 60 => "Месяц назад",
                    _ => lastPlayed.ToString(lastPlayed.Year == DateTime.Now.Year
                            ? "d MMM."
                            : "d MMM yyyy.").ToLower()
                };
            }
        }
    }
}
