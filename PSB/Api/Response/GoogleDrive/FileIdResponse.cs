using System.Text.Json.Serialization;

namespace PSB.Api.Response.GoogleDrive
{
    public class FileIdResponse
    {
        [JsonPropertyName("id")] public required string Id { get; set; }
    }
}
