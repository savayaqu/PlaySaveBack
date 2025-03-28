using System.Text.Json.Serialization;
using PSB.Models;

namespace PSB.Api.Response
{
    public class SignUpResponse
    {
        [JsonPropertyName("token")] public required string Token { get; set; }
        [JsonPropertyName("user")] public required User User { get; set; }
        [JsonPropertyName("key")] public int Key { get; set; }
    }
}
