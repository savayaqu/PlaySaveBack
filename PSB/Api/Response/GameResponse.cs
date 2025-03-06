using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Models;

namespace PSB.Api.Response
{
    public class GameResponse
    {
        [JsonPropertyName("game")] public required Game Game  { get; set; }
        [JsonPropertyName("library")] public Library? Library { get; set; }
        [JsonPropertyName("saves")] public List<Save>? Saves { get; set; }
    }
}
