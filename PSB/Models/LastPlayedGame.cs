using Microsoft.UI.Xaml.Controls;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;

namespace PSB.Models
{
    public class LastPlayedGame()
    {
        public required IGame Game { get; set; }
        public IconElement? GameIcon => IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(Game)) ?? new FontIcon { Glyph = "\uE7FC" };

        public required Library Library { get; set; }
    }
}
