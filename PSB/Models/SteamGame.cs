using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace PSB.Models
{
    public class SteamGame
    {
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("steam_appid")] public required ulong SteamAppId { get; set; }
        [JsonPropertyName("about_the_game")] public string? AboutTheGame { get; set; }
        [JsonPropertyName("pc_requirements")] public PcRequirements? PcRequirements { get; set; }
        [JsonPropertyName("screenshots")] public List<Screenshot>? Screenshots { get; set; }
        [JsonPropertyName("movies")] public List<Movie>? Movies { get; set; }
    }

    public class PcRequirements
    {
        [JsonPropertyName("minimum")] public string? Minimum { get; set; }
        [JsonPropertyName("recommended")] public string? Recommended { get; set; }
    }
    public class Screenshot
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("path_thumbnail")] public required string PathThumbnail { get; set; }
        [JsonPropertyName("path_full")] public required string PathFull { get; set; }
    }

    public class Movie
    {
        [JsonPropertyName("id")] public ulong Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("thumbnail")] public required string Thumbnail { get; set; }
        [JsonPropertyName("webm")] public required VideoFormats Webm { get; set; }
        [JsonPropertyName("mp4")] public required VideoFormats Mp4 { get; set; }
        [JsonPropertyName("highlight")] public bool Highlight { get; set; }
        public MediaSource VideoSource => MediaSource.CreateFromUri(new Uri(Mp4.ResolutionMax));

    }
    public class VideoFormats
    {
        [JsonPropertyName("480")] public required string Resolution480 { get; set; }
        [JsonPropertyName("max")] public required string ResolutionMax { get; set; }
    }
}
