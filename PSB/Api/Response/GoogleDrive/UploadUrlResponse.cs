using System.Text.Json.Serialization;

namespace PSB.Api.Response.GoogleDrive
{
    public class UploadUrlResponse
    {
        [JsonPropertyName("upload_url")] public required string UploadUrl { get; set; }
        [JsonPropertyName("save_id")] public required ulong SaveId { get; set; }
    }
}
