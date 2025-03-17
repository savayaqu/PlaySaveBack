using System.Text.Json.Serialization;
using PSB.Interfaces;

namespace PSB.Models
{
    public class SideGame : IGame
    {
        [JsonPropertyOrder(0)]  // Указываем, что это первое поле в JSON
        [JsonPropertyName("type")]
        public string Type { get; set; } = "SideGame";  // Теперь JSON его учитывает
        [JsonPropertyName("id")] public required ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }

    }
}
