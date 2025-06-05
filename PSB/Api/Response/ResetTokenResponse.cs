using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class ResetTokenResponse
    {
        [JsonPropertyName("reset_token")] public required string ResetToken { get; set; }
    }
}
