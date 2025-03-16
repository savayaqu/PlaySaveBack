using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Interfaces;
using PSB.Models;

namespace PSB.Helpers
{
    [JsonSerializable(typeof(IGame))]
    [JsonSerializable(typeof(Game))]
    [JsonSerializable(typeof(SideGame))]
    public partial class GameSerializationContext : JsonSerializerContext
    {
    }

}
