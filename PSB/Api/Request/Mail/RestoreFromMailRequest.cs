using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request.Mail
{
    public class RestoreFromMailRequest(string email, string resetToken, string password, string passwordConfirmation, bool logout)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;
        [JsonPropertyName("reset_token")] public string ResetToken { get; set; } = resetToken;
        [JsonPropertyName("new_password")] public string NewPassword { get; set; } = password;
        [JsonPropertyName("new_password_confirmation")] public string NewPasswordConfirmation { get; set; } = passwordConfirmation;
        [JsonPropertyName("logout")] public bool Logout { get; set; } = logout;

    }
}
