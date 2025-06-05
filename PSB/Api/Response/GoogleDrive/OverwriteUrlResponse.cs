using System;
using System.Text.Json.Serialization;

namespace PSB.Api.Response.GoogleDrive
{
    public class OverwriteUrlResponse
    {
        [JsonPropertyName("upload_url")] public required string UploadUrl { get; set; }
        [JsonPropertyName("expires_at")] public required DateTime ExpiresAt { get; set; }
    }
}
