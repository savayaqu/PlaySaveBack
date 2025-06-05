using PSB.Models;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class AuthResponse
    {
        [JsonPropertyName("token")] public string? Token { get; set; }
        [JsonPropertyName("user")] public User? User { get; set; }
    }
}