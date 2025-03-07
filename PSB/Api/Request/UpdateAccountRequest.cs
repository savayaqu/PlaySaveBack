using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request
{
    public class UpdateAccountRequest
    {
        public class UpdatePasswordRequest(string currentPassword, string newPassword, string newPasswordConfirmation)
        {
            [JsonPropertyName("current_password")] public string CurrentPassword { get; set; } = currentPassword;
            [JsonPropertyName("new_password")] public string NewPassword { get; set; } = newPassword;
            [JsonPropertyName("new_password_confirmation")] public string NewPasswordConfirmation { get; set; } = newPasswordConfirmation;
        }
        public class UpdateEmailRequest(string email)
        {
            [JsonPropertyName("email")] public string Email { get; set; } = email;
        }
    }
}
