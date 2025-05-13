using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Models
{
    public class GroupedSaves
    {
        public string GameName { get; set; } = string.Empty;
        public List<Save> Saves { get; set; } = [];
    }
}
