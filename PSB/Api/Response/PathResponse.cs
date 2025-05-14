using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class PathResponse
    {
        [JsonPropertyName("path")] public required string Path{ get; set; }
    }
}
