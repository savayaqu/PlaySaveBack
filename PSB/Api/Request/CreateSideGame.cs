using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Utils;

namespace PSB.Api.Request
{
    public class CreateSideGame(string name)
    {
        [JsonPropertyName("name")] public string Name { get; set; } = name;
    }
}
