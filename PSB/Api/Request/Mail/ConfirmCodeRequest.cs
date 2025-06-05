using System.Text.Json.Serialization;

namespace PSB.Api.Request.Mail
{
    public class ConfirmCodeRequest(string email, string code)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;
        [JsonPropertyName("code")] public string Code { get; set; } = code;

    }
}
