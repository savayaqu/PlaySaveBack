using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request
{
    public class UpdateSaveRequest(string version, string description, string hash, DateTime lastSyncAt)
    {
        [JsonPropertyName("version")] public string Name { get; set; } = version;
        [JsonPropertyName("description")] public string Description { get; set; } = description;
        [JsonPropertyName("hash")] public string Hash { get; set; } = hash;
        [JsonPropertyName("last_sync_at")] public DateTime LastSyncAt { get; set; } = lastSyncAt;

    }
}
