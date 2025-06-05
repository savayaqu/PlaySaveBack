using PSB.Interfaces;

namespace PSB.Models
{
    public class LastPlayedGame()
    {
        public required IGame Game { get; set; }
        public required Library Library { get; set; }
    }
}
