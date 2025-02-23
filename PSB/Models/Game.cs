using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PSB.Models
{
    public class Game
    {
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("icon")] public required string Icon { get; set; }
        [JsonPropertyName("description")] public required string Description { get; set; }        
    }
}
