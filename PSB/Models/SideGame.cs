using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Models
{
    public class SideGame
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; } 
    }
}
