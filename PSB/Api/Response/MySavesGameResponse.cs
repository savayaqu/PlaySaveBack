using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Models;

namespace PSB.Api.Response
{
    public class MySavesGameResponse
    {
        [JsonPropertyName("saves")] public List<Save> Save { get; set; } = new();
    }
}
