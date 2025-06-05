using PSB.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class StatisticResponse
    {
        [JsonPropertyName("totalPlayed")]
        public uint TotalPlayed { get; set; }

        [JsonPropertyName("recentlyPlayed")]
        public required List<RecentlyPlayed> RecentlyPlayed { get; set; }
    }
}
