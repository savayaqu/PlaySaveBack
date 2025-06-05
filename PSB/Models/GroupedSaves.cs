using Microsoft.UI.Xaml.Controls;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;
using System.Collections.Generic;

namespace PSB.Models
{
    public class GroupedSaves
    {
        public required IGame Game { get; set; }
        public IconElement? GameIcon => IconFromExe.GetIconElement(PathDataManager<IGame>.GetFilePath(Game)) ?? new FontIcon { Glyph = "\uE7FC" };

        public List<Save> Saves { get; set; } = [];
    }
}
