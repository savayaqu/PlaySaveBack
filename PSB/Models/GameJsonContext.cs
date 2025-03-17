using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Interfaces;

namespace PSB.Models
{
    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(IGame))]
    [JsonSerializable(typeof(Game))]
    [JsonSerializable(typeof(SideGame))]
    [JsonSerializable(typeof(Library))]
    [JsonSerializable(typeof(List<Save>))] 
    public partial class GameJsonContext : JsonSerializerContext
    {
    }
}
