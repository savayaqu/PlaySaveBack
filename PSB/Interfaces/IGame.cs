using PSB.Models;
using System.Text.Json.Serialization;

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
