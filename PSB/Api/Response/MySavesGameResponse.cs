using PSB.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class MySavesGameResponse
    {
        [JsonPropertyName("saves")] public List<Save> Save { get; set; } = new();
    }
}
