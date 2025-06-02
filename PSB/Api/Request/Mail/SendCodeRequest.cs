using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request.Mail
{
    public class SendCodeRequest(string email)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;

    }
}
