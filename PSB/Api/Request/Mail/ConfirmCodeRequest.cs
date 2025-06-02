using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request.Mail
{
    public class ConfirmCodeRequest(string email, string code)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;
        [JsonPropertyName("code")] public string Code { get; set; } = code;

    }
}
