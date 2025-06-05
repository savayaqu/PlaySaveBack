using System.Text.Json.Serialization;

namespace PSB.Api.Response.GoogleDrive.Save
{
    public class ShareSaveResponse
    {
        [JsonPropertyName("url")] public required string Url { get; set; }
    }
}
