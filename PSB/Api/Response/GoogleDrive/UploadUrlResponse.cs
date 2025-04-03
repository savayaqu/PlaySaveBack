using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Response.GoogleDrive
{
    public class UploadUrlResponse
    {
        [JsonPropertyName("upload_url")] public required string UploadUrl { get; set; }
        [JsonPropertyName("save_id")] public required ulong SaveId{ get; set; }
        [JsonPropertyName("expires_at")] public required DateTime ExpiresAt{ get; set; }
    }
}
