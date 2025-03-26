using System.Text.Json.Serialization;

namespace PSB.Api.Request
{
    public class CreateSideGame(string name)
    {
        [JsonPropertyName("name")] public string Name { get; set; } = name;
    }
}
