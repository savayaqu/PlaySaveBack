using System;
using System.Text.Json.Serialization;

namespace PSB.Models
{
    public class CloudService
    {
        [JsonPropertyName("id")] public ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("icon")] public required string Icon { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("is_connected")] public Boolean IsConnected { get; set; }
        [JsonPropertyName("expires_at")] string? ExpiresAt { get; set; }

        // Добавляем вычисляемое свойство для статуса
        public string ConnectionStatus
        {
            get
            {
                if (!IsConnected) return "Не подключен";
                if (IsTokenExpired()) return "Токен истёк";
                return "Подключен";
            }
        }

        public bool IsTokenExpired()
        {
            if (string.IsNullOrEmpty(ExpiresAt)) return false;
            return DateTime.Parse(ExpiresAt) < DateTime.UtcNow;
        }
        // Показывать кнопку или нет
        public bool ShowConnectButton =>
        !IsConnected || IsTokenExpired() || Name == "Google Drive";
        // Свойство для текста кнопки
        public string ConnectButtonText
        {
            get
            {
                if (!IsConnected) return "Подключить";
                if (IsTokenExpired()) return "Обновить токен";
                return "Отключить";
            }
        }
    }
}
