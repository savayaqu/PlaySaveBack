using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request
{
    public class CredentialsRequest(string email, string password)
    {
        [JsonPropertyName("email")] public string Email { get; set; } = email;
        [JsonPropertyName("password")] public string Password { get; set; } = password;
    }
}
