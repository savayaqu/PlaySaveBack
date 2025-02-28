using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request
{
    public class UpdateLibraryGameRequest(uint? time_played, string? last_played_at)
    {
        [JsonPropertyName("time_played")]    public uint? Identifier { get; set; } = time_played;
        [JsonPropertyName("last_played_at")] public string? LastPlatedAt { get; set; } = last_played_at;
    }
}
