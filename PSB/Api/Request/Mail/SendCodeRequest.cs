using System.Text.Json.Serialization;

namespace PSB.Api.Request.Mail
{
    public class SendCodeRequest(string email)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;

    }
}
