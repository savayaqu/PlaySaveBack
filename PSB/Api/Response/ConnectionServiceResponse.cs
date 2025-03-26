using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class ConnectionServiceResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("url")] public required string Url { get; set; }

    }
}
