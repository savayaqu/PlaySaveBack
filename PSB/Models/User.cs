using System.Text.Json.Serialization;

namespace PSB.Models
{
    public class User
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("nickname")] public required string Nickname { get; set; }
        [JsonPropertyName("header")] public required string Header { get; set; }
        [JsonPropertyName("avatar")] public required string Avatar { get; set; }
        [JsonPropertyName("login")] public required string Login { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("visibility")] public required int Visibility { get; set; }

    }
}
