using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Converters;

namespace PSB.Models
{
    public class Save
    {
        [JsonPropertyName("id")] public uint Id{ get; set; }
        [JsonPropertyName("file_id")] public required string FileId{ get; set; }
        [JsonPropertyName("file_name")] public required string FileName { get; set; }
        [JsonPropertyName("version")] public required string Version { get; set; }
        [JsonPropertyName("size")] public uint Size { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("user_cloud_service_id")] public uint UserCloudServiceId{ get; set; }

        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        [JsonConverter(typeof(CustomDateTimeConverter))] 
        public DateTime UpdatedAt { get; set; }
    }
}
