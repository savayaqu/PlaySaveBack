using System.Text.Json.Serialization;
using PSB.Models;

namespace PSB.Api.Response
{
    public class ResetTokenResponse
    {
        [JsonPropertyName("reset_token")] public required string ResetToken { get; set; }
    }
}
