using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request.GoogleDrive
{
    public class ConfirmUploadRequest(string fileId, string fileHash )
    {
        [JsonPropertyName("file_id")] public string FileId { get; set; } = fileId;
        [JsonPropertyName("file_hash")] public string FileHash { get; set; } = fileHash;
    }
}
