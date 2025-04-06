using System.Text.Json.Serialization;

namespace PSB.Api.Request
{
    public class RestoreFromKeyRequest(string login, string key, string password, string passwordConfirmation, bool logout)
    {
        [JsonPropertyName("login")] public string Login { get; set; } = login;
        [JsonPropertyName("key")] public string Key { get; set; } = key;
        [JsonPropertyName("new_password")] public string NewPassword { get; set; } = password;
        [JsonPropertyName("new_password_confirmation")] public string NewPasswordConfirmation { get; set; } = passwordConfirmation;
        [JsonPropertyName("logout")] public bool Logout { get; set; } = logout;
    }
}
