using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Api.Request
{
    public class CredentialsRequest(string identifier, string password)
    {
        [JsonPropertyName("identifier")] public string Identifier { get; set; } = identifier;
        [JsonPropertyName("password")] public string Password { get; set; } = password;
    }
}
