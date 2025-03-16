using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Models
{
    public class GameNavigationParameters
    {
        public required string Type { get; set; }
        public ulong GameId { get; set; }
    }
}
