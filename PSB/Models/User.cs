using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Models
{
    public class User
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("email")] public required string Email { get; set; }
        [JsonPropertyName("password")] public string? Password { get; set; }
        [JsonPropertyName("avatar")] public string? Avatar { get; set; }
        [JsonPropertyName("is_admin")] public int? IsAdmin { get; set; }
        [JsonPropertyName("nickname")] public required string Nickname { get; set; }
    }
}
