using System.Text.Json.Serialization;
using PSB.Models;

namespace PSB.Api.Response
{
    public class AuthResponse
    {
        [JsonPropertyName("token")] public string? Token { get; set; }
        [JsonPropertyName("user")] public User? User { get; set; }
    }
}
