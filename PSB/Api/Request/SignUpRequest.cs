using System.Text.Json.Serialization;

namespace PSB.Api.Request
{
    public class SignUpRequest(string login, string? email, string password, string passwordConfirmation)
    {
        [JsonPropertyName("login")] public string Login { get; set; } = login;
        [JsonPropertyName("email")] public string? Email { get; set; } = email;
        [JsonPropertyName("password")] public string Password { get; set; } = password;
        [JsonPropertyName("password_confirmation")] public string PasswordConfirmation { get; set; } = passwordConfirmation;
    }
}
