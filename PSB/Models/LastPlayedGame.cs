using Microsoft.UI.Xaml.Controls;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Utils.Game;

namespace PSB.Models
{
    public class LastPlayedGame()
    {
        public required IGame Game { get; set; }
        public required Library Library { get; set; }
    }
}
