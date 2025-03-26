using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class ErrorResponse
    {
        [JsonPropertyName("code")] public int? Code { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("errors")] public Dictionary<string, List<string>>? Errors { get; set; }
    }
}
