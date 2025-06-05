using Microsoft.UI.Xaml.Controls;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;
using System.Text.Json.Serialization;

namespace PSB.Models
{
    public class RecentlyPlayed
    {
        [JsonPropertyName("time_played")]
        public uint TimePlayed { get; set; }

        [JsonPropertyName("game")]
        public Game? Game { get; set; }
        [JsonPropertyName("sideGame")]
        public SideGame? SideGame { get; set; }
        public IGame UnifiedGame => Game ?? (IGame)SideGame!;
        public IconElement? UnifiedImage => IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(UnifiedGame)) ?? new FontIcon { Glyph = "\uE7FC" };

    }
}
