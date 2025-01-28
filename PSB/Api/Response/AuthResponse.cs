using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Models;

namespace PSB.Api.Response
{
    public class AuthResponse
    {
        [JsonPropertyName("token")] public string? Token { get; set; }
        [JsonPropertyName("user")] public User? User { get; set; }
    }
}
