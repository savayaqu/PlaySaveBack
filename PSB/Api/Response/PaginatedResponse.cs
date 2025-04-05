using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PSB.Api.Response
{
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("data")] public List<T> Data { get; set; } = new();
        [JsonPropertyName("links")] public PaginationLinks Links { get; set; } = new();
        [JsonPropertyName("meta")] public PaginationMeta Meta { get; set; } = new();
    }

    public class PaginationLinks
    {
        [JsonPropertyName("first")] public string? First { get; set; }
        [JsonPropertyName("last")]  public string? Last { get; set; }
        [JsonPropertyName("prev")]  public string? Prev { get; set; }
        [JsonPropertyName("next")]  public string? Next { get; set; }
    }

    public class PaginationMeta
    {
        [JsonPropertyName("current_page")] public int? CurrentPage { get; set; }
        [JsonPropertyName("from")] public int? From { get; set; }
        [JsonPropertyName("last_page")] public int? LastPage { get; set; }
        [JsonPropertyName("per_page")] public int? PerPage { get; set; }
        [JsonPropertyName("to")] public int? To { get; set; }
        [JsonPropertyName("total")] public int? Total { get; set; }
    }
}
