using System.Text.Json.Serialization;

namespace PSB.Api.Request
{
    public class CredentialsRequest(string identifier, string password)
    {
        [JsonPropertyName("identifier")] public string Identifier { get; set; } = identifier;
        [JsonPropertyName("password")] public string Password { get; set; } = password;
    }
}
