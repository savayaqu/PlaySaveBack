using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using PSB.Converters;
using PSB.Utils;

namespace PSB.Models
{
    public class Save
    {
        [JsonPropertyName("id")] public uint Id { get; set; }
        [JsonPropertyName("file_id")] public string? FileId { get; set; }
        [JsonPropertyName("file_name")] public required string FileName { get; set; }
        [JsonPropertyName("version")] public required string Version { get; set; }
        [JsonPropertyName("size")] public ulong Size { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("hash")] public string? Hash { get; set; }
        [JsonPropertyName("last_sync_at")] public string? LastSyncAt { get; set; }
        [JsonPropertyName("game_id")] public ulong? GameId { get; set; }
        [JsonPropertyName("side_game_id")] public ulong? SideGameId { get; set; }
        [JsonPropertyName("user_cloud_service_id")] public ulong UserCloudServiceId { get; set; }
        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        private bool? _isSynced; // backing field для ручного управления статусом
        [JsonIgnore]
        public bool IsSynced
        {
            get => _isSynced ?? !string.IsNullOrEmpty(LastSyncAt) || !string.IsNullOrEmpty(FileId);
            set
            {
                _isSynced = value;
            }
        }
        [JsonIgnore] public string? ZipPath { get; set; }
        [JsonIgnore] public string? Backup { get; set; }
        public DateTime? LastRestored { get; set; }
        [JsonIgnore]
        public CloudService? CloudService
        {
            get
            {
              return AuthData.ConnectedCloudServices.FirstOrDefault(ccc => ccc.UserCloudServiceId == UserCloudServiceId && ccc.IsConnected);
            }
        }
        [JsonIgnore]
        public string SyncIconColor => IsSynced ? "#FF4CAF50" : "#FFFFC107"; // Зеленый для синхронизированного, желтый для ожидания
    }
}