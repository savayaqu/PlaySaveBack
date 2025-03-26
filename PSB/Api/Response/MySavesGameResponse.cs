using System.Collections.Generic;
using System.Text.Json.Serialization;
using PSB.Models;

namespace PSB.Api.Response
{
    public class MySavesGameResponse
    {
        [JsonPropertyName("saves")] public List<Save> Save { get; set; } = new();
    }
}
