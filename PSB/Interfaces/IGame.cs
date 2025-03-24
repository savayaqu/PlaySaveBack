using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PSB.Models;

namespace PSB.Interfaces
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(Game), typeDiscriminator: "game")]
    [JsonDerivedType(typeof(SideGame), typeDiscriminator: "sidegame")]
    public interface IGame
    {
        [JsonPropertyName("id")]
        ulong Id { get; set; }

        [JsonPropertyName("name")]
        string Name { get; set; }

        [JsonPropertyName("type")]
        string Type { get; set; }
    }

}
