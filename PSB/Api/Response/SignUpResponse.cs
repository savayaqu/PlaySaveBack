using PSB.Models;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class SignUpResponse
    {
        [JsonPropertyName("token")] public required string Token { get; set; }
        [JsonPropertyName("user")] public required User User { get; set; }
        [JsonPropertyName("key")] public required string Key { get; set; }
    }
}
